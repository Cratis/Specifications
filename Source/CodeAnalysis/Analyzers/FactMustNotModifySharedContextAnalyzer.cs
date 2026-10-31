// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Cratis.Specifications.CodeAnalysis.Analyzers;

/// <summary>
/// Analyzer that reports when a fact provably modifies state shared through the specification instance.
/// </summary>
/// <remarks>
/// Reported as an error, because specifications share one context instance by default. It reports assignments,
/// increments and decrements of a field or property of the specification, or of anything reached through one
/// (<c>_x.Y = ...</c>, <c>_items[0] = ...</c>); <c>Add</c>, <c>AddRange</c>, <c>Remove</c>, <c>RemoveAt</c>,
/// <c>Clear</c> and <c>Insert</c> calls on such a field; disposing it; reconfiguring a shared NSubstitute substitute;
/// and calling a method of the specification, declared in the same file, that does any of these.
/// Specifications marked with <c>PerFactSpecificationLifecycle</c> (on the class, a containing or base class, or the
/// assembly) and theories are not analyzed. Lower the severity through <c>dotnet_diagnostic.CRSPEC0008.severity</c> in an .editorconfig.
/// Calls that only probably change the shared state are reported as warnings by <see cref="FactShouldNotCallStateChangingMembersOnSharedContextAnalyzer"/>.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class FactMustNotModifySharedContextAnalyzer : DiagnosticAnalyzer
{
    static readonly DiagnosticDescriptor _rule = new(
        id: DiagnosticIds.FactMustNotModifySharedContext,
        title: "Facts should not modify the shared specification context",
        messageFormat: "Fact '{0}' modifies the shared specification state '{1}'{2}; move the change into Establish or Because, or mark the class [PerFactSpecificationLifecycle] so each fact gets its own context",
        category: "Reliability",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "All facts of a specification share one context instance that is established once. A fact that assigns to, or mutates, a field or property of that context, or an object reached through one, makes other facts depend on execution order. Move the change into Establish or Because, use a local variable, or mark the class [PerFactSpecificationLifecycle].");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [_rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(compilationContext =>
        {
            if (!SharedLifecycleFacts.AppliesTo(compilationContext.Compilation))
            {
                return;
            }

            compilationContext.RegisterSyntaxNodeAction(
                Analyze,
                SyntaxKind.SimpleAssignmentExpression,
                SyntaxKind.AddAssignmentExpression,
                SyntaxKind.SubtractAssignmentExpression,
                SyntaxKind.MultiplyAssignmentExpression,
                SyntaxKind.DivideAssignmentExpression,
                SyntaxKind.ModuloAssignmentExpression,
                SyntaxKind.AndAssignmentExpression,
                SyntaxKind.OrAssignmentExpression,
                SyntaxKind.ExclusiveOrAssignmentExpression,
                SyntaxKind.LeftShiftAssignmentExpression,
                SyntaxKind.RightShiftAssignmentExpression,
                SyntaxKind.CoalesceAssignmentExpression,
                SyntaxKind.PreIncrementExpression,
                SyntaxKind.PostIncrementExpression,
                SyntaxKind.PreDecrementExpression,
                SyntaxKind.PostDecrementExpression,
                SyntaxKind.InvocationExpression);
        });
    }

    static void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (!SharedLifecycleFacts.TryGetSharedFact(context, out var fact))
        {
            return;
        }

        var members = new SharedMembers(context.SemanticModel, fact.ContainingType, context.CancellationToken);
        if (new SharedStateWriteFinder(members).Find(context.Node) is { } change)
        {
            context.ReportDiagnostic(Diagnostic.Create(_rule, MemberCall.ResultOf((ExpressionSyntax)context.Node).GetLocation(), fact.Name, change.Member, change.Through));
        }
    }
}
