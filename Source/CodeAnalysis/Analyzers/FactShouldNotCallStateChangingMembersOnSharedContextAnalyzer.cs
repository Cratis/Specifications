// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Cratis.Specifications.CodeAnalysis.Analyzers;

/// <summary>
/// Analyzer that reports when a fact calls a member on the shared specification context that, by its name, changes it.
/// </summary>
/// <remarks>
/// Reported as a warning, because the analyzer judges the call by the member's name, not by what it does. It covers
/// running an action (<c>Execute</c>, <c>Handle</c>), consuming reads (<c>Take</c>, <c>Dequeue</c>, <c>Pop</c>,
/// <c>MoveNext</c>), and collection changes (<c>Push</c>, <c>Enqueue</c>, <c>TryAdd</c>, <c>Reset</c>, and
/// <c>Add</c>/<c>Remove</c>/<c>Clear</c>-style calls on an object reached through a field, such as <c>_x.Items.Clear()</c>).
/// LINQ operators, immutable collections, calls verified through <c>Received()</c> or <c>Received.InOrder</c>, theories,
/// and specifications marked <c>PerFactSpecificationLifecycle</c> are not reported. Provable changes are reported by
/// <see cref="FactMustNotModifySharedContextAnalyzer"/>.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class FactShouldNotCallStateChangingMembersOnSharedContextAnalyzer : DiagnosticAnalyzer
{
    const string LinqNamespace = "System.Linq";

    // Names that change their receiver whether or not the result is used.
    static readonly HashSet<string> _stateChangingNames = new(System.StringComparer.Ordinal)
    {
        "Execute",
        "ExecuteAsync",
        "Handle",
        "HandleAsync",
        "Take",
        "TryTake",
        "Dequeue",
        "TryDequeue",
        "Pop",
        "TryPop",
        "MoveNext",
        "Push",
        "Enqueue",
        "TryAdd",
        "TryRemove",
        "TryUpdate",
        "AddOrUpdate",
        "RemoveAll",
        "Reset"
    };

    // Names CRSPEC0008 reports on a shared field itself; here they are reported on objects reached through one.
    static readonly HashSet<string> _nestedCollectionMutatorNames = new(System.StringComparer.Ordinal)
    {
        "Add",
        "AddRange",
        "Remove",
        "RemoveAt",
        "Clear",
        "Insert"
    };

    static readonly DiagnosticDescriptor _rule = new(
        id: DiagnosticIds.FactShouldNotCallStateChangingMembersOnSharedContext,
        title: "Facts should not call members that change the shared specification context",
        messageFormat: "Fact '{0}' calls '{1}' on the shared specification state '{2}', which changes it for the other facts; move the call into Because, or mark the class [PerFactSpecificationLifecycle] so each fact gets its own context",
        category: "Reliability",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "All facts of a specification share one context instance that is established once. Running an action, consuming a queue or stack, or changing a nested collection from a fact makes other facts depend on execution order. The analyzer judges the call by the member's name, so it is a warning.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [_rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(compilationContext =>
        {
            if (SharedLifecycleFacts.AppliesTo(compilationContext.Compilation))
            {
                compilationContext.RegisterSyntaxNodeAction(Analyze, SyntaxKind.InvocationExpression);
            }
        });
    }

    static void Analyze(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess || !IsCandidate(invocation, memberAccess))
        {
            return;
        }

        if (!SharedLifecycleFacts.TryGetSharedFact(context, out var fact) || IsExempt(context, invocation, memberAccess))
        {
            return;
        }

        var members = new SharedMembers(context.SemanticModel, fact.ContainingType, context.CancellationToken);
        if (members.Root(memberAccess.Expression, throughInvocations: false) is { } member)
        {
            context.ReportDiagnostic(Diagnostic.Create(_rule, invocation.GetLocation(), fact.Name, memberAccess.Name.Identifier.ValueText, member));
        }
    }

    static bool IsCandidate(InvocationExpressionSyntax invocation, MemberAccessExpressionSyntax memberAccess)
    {
        var name = memberAccess.Name.Identifier.ValueText;
        return _stateChangingNames.Contains(name) || (_nestedCollectionMutatorNames.Contains(name) && IsDiscardedCallOnNestedObject(invocation, memberAccess));
    }

    static bool IsDiscardedCallOnNestedObject(InvocationExpressionSyntax invocation, MemberAccessExpressionSyntax memberAccess) =>
        !SharedMembers.IsDirect(memberAccess.Expression) && SharedLifecycleFacts.IsDiscardedResult(invocation);

    // Immutable collections and LINQ operators return a new value; calls inside Received.InOrder are recorded, not run.
    static bool IsExempt(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation, MemberAccessExpressionSyntax memberAccess) =>
        SharedLifecycleFacts.IsImmutableCollection(memberAccess.Expression, context.SemanticModel, context.CancellationToken) ||
        IsLinq(context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol) ||
        SharedLifecycleFacts.IsInsideReceivedInOrder(invocation, context.SemanticModel, context.CancellationToken);

    static bool IsLinq(ISymbol? symbol) =>
        symbol?.ContainingNamespace?.ToDisplayString() is { } ns &&
        (ns == LinqNamespace || ns.StartsWith(LinqNamespace + ".", System.StringComparison.Ordinal));
}
