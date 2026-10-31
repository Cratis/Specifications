// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cratis.Specifications.CodeAnalysis.Analyzers;

/// <summary>
/// Finds code that provably changes state a specification shares between its facts.
/// </summary>
/// <param name="members">Resolves the shared members of the specification.</param>
sealed class SharedStateWriteFinder(SharedMembers members)
{
    static readonly HashSet<string> _collectionMutatorNames = new(System.StringComparer.Ordinal)
    {
        "Add",
        "AddRange",
        "Remove",
        "RemoveAt",
        "Clear",
        "Insert"
    };

    static readonly HashSet<string> _substituteConfigurationNames = new(System.StringComparer.Ordinal)
    {
        "Returns",
        "ReturnsForAnyArgs",
        "ReturnsNull",
        "ReturnsNullForAnyArgs",
        "Throws",
        "ThrowsForAnyArgs",
        "ThrowsAsync",
        "ThrowsAsyncForAnyArgs",
        "When",
        "WhenForAnyArgs",
        "ClearReceivedCalls",
        "ClearSubstitute"
    };

    readonly HashSet<IMethodSymbol> _visitedHelpers = new(SymbolEqualityComparer.Default);

    /// <summary>
    /// Gets the shared member the node changes.
    /// </summary>
    /// <param name="node">An assignment, increment, decrement or invocation.</param>
    /// <returns>The change, or null if the node does not change shared state.</returns>
    public SharedStateChange? Find(SyntaxNode node) => node switch
    {
        AssignmentExpressionSyntax assignment => Changed(members.Root(assignment.Left, throughInvocations: false)),
        PrefixUnaryExpressionSyntax prefix when IsIncrementOrDecrement(prefix) => Changed(members.Root(prefix.Operand, throughInvocations: false)),
        PostfixUnaryExpressionSyntax postfix when IsIncrementOrDecrement(postfix) => Changed(members.Root(postfix.Operand, throughInvocations: false)),
        InvocationExpressionSyntax invocation when !SharedLifecycleFacts.IsInsideReceivedInOrder(invocation, members.Model, members.CancellationToken) => FindInCall(invocation),
        _ => null
    };

    static SharedStateChange? Changed(string? member) => member is null ? null : new SharedStateChange(member, null);

    static bool IsIncrementOrDecrement(ExpressionSyntax expression) =>
        expression.Kind() is SyntaxKind.PreIncrementExpression or SyntaxKind.PreDecrementExpression or
            SyntaxKind.PostIncrementExpression or SyntaxKind.PostDecrementExpression;

    static bool IsDispose(IMethodSymbol method) =>
        method.Parameters.Length == 0 &&
        (method.Name == "Dispose" ? Implements(method.ContainingType, "System.IDisposable") :
         method.Name == "DisposeAsync" && Implements(method.ContainingType, "System.IAsyncDisposable"));

    static bool Implements(INamedTypeSymbol type, string interfaceName) =>
        type.ToDisplayString() == interfaceName || type.AllInterfaces.Any(_ => _.ToDisplayString() == interfaceName);

    static bool IsSubstituteConfiguration(IMethodSymbol method) =>
        _substituteConfigurationNames.Contains(method.Name) &&
        method.ContainingNamespace?.ToDisplayString() is { } ns &&
        (ns == "NSubstitute" || ns.StartsWith("NSubstitute.", System.StringComparison.Ordinal));

    SharedStateChange? FindInCall(InvocationExpressionSyntax invocation)
    {
        var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
        if (FindCollectionMutation(invocation, memberAccess) is { } mutated)
        {
            return mutated;
        }

        if (members.Model.GetSymbolInfo(invocation, members.CancellationToken).Symbol is not IMethodSymbol method)
        {
            return null;
        }

        return FindDisposal(memberAccess, method) ?? FindSubstituteConfiguration(invocation, method) ?? FindInHelper(invocation, method);
    }

    SharedStateChange? FindCollectionMutation(InvocationExpressionSyntax invocation, MemberAccessExpressionSyntax? memberAccess)
    {
        if (memberAccess is null || !IsDiscardedCollectionMutatorCall(invocation, memberAccess))
        {
            return null;
        }

        return SharedLifecycleFacts.IsImmutableCollection(memberAccess.Expression, members.Model, members.CancellationToken)
            ? null
            : Changed(members.Direct(memberAccess.Expression));
    }

    static bool IsDiscardedCollectionMutatorCall(InvocationExpressionSyntax invocation, MemberAccessExpressionSyntax memberAccess) =>
        _collectionMutatorNames.Contains(memberAccess.Name.Identifier.ValueText) && SharedLifecycleFacts.IsDiscardedResult(invocation);

    SharedStateChange? FindDisposal(MemberAccessExpressionSyntax? memberAccess, IMethodSymbol method) =>
        memberAccess is not null && IsDispose(method) ? Changed(members.Root(memberAccess.Expression, throughInvocations: false)) : null;

    SharedStateChange? FindSubstituteConfiguration(InvocationExpressionSyntax invocation, IMethodSymbol method) =>
        IsSubstituteConfiguration(method) && GetSubstitute(invocation, method) is { } substitute
            ? Changed(members.Root(substitute, throughInvocations: true))
            : null;

    static ExpressionSyntax? GetSubstitute(InvocationExpressionSyntax invocation, IMethodSymbol method) =>
        method.ReducedFrom is not null && invocation.Expression is MemberAccessExpressionSyntax memberAccess
            ? memberAccess.Expression
            : invocation.ArgumentList.Arguments.FirstOrDefault()?.Expression;

    static bool IsCalledOnThisInstance(InvocationExpressionSyntax invocation) =>
        invocation.Expression is SimpleNameSyntax or MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax };

    static bool IsInstanceMethod(IMethodSymbol method) => !method.IsStatic && method.MethodKind == MethodKind.Ordinary;

    // A non-static method of the specification called on this instance, whose body is in the same file, is followed,
    // because its effect on the shared state is the fact's effect. Lambdas and local functions in it are not followed.
    SharedStateChange? FindInHelper(InvocationExpressionSyntax invocation, IMethodSymbol method)
    {
        if (!IsCalledOnThisInstance(invocation) || !IsInstanceMethod(method))
        {
            return null;
        }

        if (!members.IsMemberOfSpecification(method) || !_visitedHelpers.Add(method.OriginalDefinition))
        {
            return null;
        }

        var changed = GetBodiesInThisFile(method)
            .SelectMany(_ => _.DescendantNodes(node => node is not (AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax)))
            .Select(Find)
            .FirstOrDefault(_ => _ is not null);
        return changed is null ? null : new SharedStateChange(changed.Member, method.Name);
    }

    // The semantic model only answers for the analyzed file, so helpers declared elsewhere are not followed.
    IEnumerable<SyntaxNode> GetBodiesInThisFile(IMethodSymbol method) =>
        method.OriginalDefinition.DeclaringSyntaxReferences
            .Where(_ => _.SyntaxTree == members.Model.SyntaxTree)
            .Select(_ => _.GetSyntax(members.CancellationToken))
            .OfType<MethodDeclarationSyntax>()
            .Select(_ => (SyntaxNode?)_.Body ?? _.ExpressionBody)
            .Where(_ => _ is not null)
            .Select(_ => _!);
}
