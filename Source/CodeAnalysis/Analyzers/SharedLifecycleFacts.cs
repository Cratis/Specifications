// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Cratis.Specifications.CodeAnalysis.Analyzers;

/// <summary>
/// Decides whether code runs in a fact that shares its specification context with the other facts of the class.
/// </summary>
static class SharedLifecycleFacts
{
    const string PerFactAttributeName = "PerFactSpecificationLifecycleAttribute";
    const string TheoryAttributeName = "TheoryAttribute";
    const string ImmutableCollectionsNamespace = "System.Collections.Immutable";
    const string ReceivedTypeName = "NSubstitute.Received";

    /// <summary>
    /// Determines whether the compilation contains specifications sharing their context, so the analysis applies at all.
    /// </summary>
    /// <param name="compilation">The compilation to inspect.</param>
    /// <returns>True if specifications exist and the assembly is not opted out of the shared lifecycle.</returns>
    public static bool AppliesTo(Compilation compilation) =>
        compilation.GetTypeByMetadataName(SpecificationFacts.SpecificationBaseTypeName) is not null &&
        !HasPerFactAttribute(compilation.Assembly.GetAttributes());

    /// <summary>
    /// Gets the fact the analyzed node belongs to, when that fact shares its specification context.
    /// </summary>
    /// <param name="context">The analysis context.</param>
    /// <param name="fact">The fact method.</param>
    /// <returns>True if the node is inside a fact under the shared lifecycle.</returns>
    public static bool TryGetSharedFact(SyntaxNodeAnalysisContext context, out IMethodSymbol fact)
    {
        fact = null!;
        if (context.ContainingSymbol is not IMethodSymbol { MethodKind: MethodKind.Ordinary } method || !IsFactSharingItsContext(method))
        {
            return false;
        }

        fact = method;
        return true;
    }

    /// <summary>
    /// Determines whether the node sits in a delegate passed to <c>NSubstitute.Received.InOrder</c>, where calls are recorded for verification instead of executed.
    /// </summary>
    /// <param name="node">The node to inspect.</param>
    /// <param name="model">The semantic model of the node's tree.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the node is inside a <c>Received.InOrder</c> delegate.</returns>
    public static bool IsInsideReceivedInOrder(SyntaxNode node, SemanticModel model, CancellationToken cancellationToken)
    {
        for (var ancestor = node.Parent; ancestor is not null and not MethodDeclarationSyntax; ancestor = ancestor.Parent)
        {
            if (ancestor is AnonymousFunctionExpressionSyntax
                {
                    Parent: ArgumentSyntax { Parent: ArgumentListSyntax { Parent: InvocationExpressionSyntax invocation } }
                } && IsReceivedInOrder(invocation, model, cancellationToken))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the result of the invocation is discarded. A call whose result is used (fluent chain, assignment, argument) is not treated as a mutation.
    /// </summary>
    /// <param name="invocation">The invocation to inspect.</param>
    /// <returns>True if the result is discarded.</returns>
    public static bool IsDiscardedResult(InvocationExpressionSyntax invocation) =>
        invocation.Parent is ExpressionStatementSyntax or ArrowExpressionClauseSyntax;

    /// <summary>
    /// Determines whether the expression's type is an immutable collection, whose members return a new collection and leave the receiver untouched.
    /// </summary>
    /// <param name="expression">The receiver expression.</param>
    /// <param name="model">The semantic model of the expression's tree.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the receiver is an immutable collection.</returns>
    public static bool IsImmutableCollection(ExpressionSyntax expression, SemanticModel model, CancellationToken cancellationToken) =>
        model.GetTypeInfo(expression, cancellationToken).Type?.ContainingNamespace?.ToDisplayString() == ImmutableCollectionsNamespace;

    static bool IsReceivedInOrder(InvocationExpressionSyntax invocation, SemanticModel model, CancellationToken cancellationToken) =>
        model.GetSymbolInfo(invocation, cancellationToken).Symbol is IMethodSymbol { Name: "InOrder" } method &&
        SymbolEqualityComparer.Default.Equals(method.ContainingType, model.Compilation.GetTypeByMetadataName(ReceivedTypeName));

    static bool IsFactSharingItsContext(IMethodSymbol method) => IsSpecificationFact(method) && !IsPerFactOrTheory(method);

    static bool IsSpecificationFact(IMethodSymbol method) =>
        SpecificationFacts.DerivesFromSpecification(method.ContainingType) && SpecificationFacts.IsTestMethod(method);

    static bool IsPerFactOrTheory(IMethodSymbol method) =>
        HasAttribute(method.GetAttributes(), TheoryAttributeName, inherit: true) || IsPerFact(method.ContainingType);

    static bool IsPerFact(INamedTypeSymbol type)
    {
        for (var container = type; container is not null; container = container.ContainingType)
        {
            for (var current = container; current is not null; current = current.BaseType)
            {
                if (HasPerFactAttribute(current.GetAttributes()))
                {
                    return true;
                }
            }
        }

        return false;
    }

    static bool HasPerFactAttribute(ImmutableArray<AttributeData> attributes) => HasAttribute(attributes, PerFactAttributeName, inherit: false);

    // Matches by metadata name so it works for the xUnit and xUnit v3 packages alike, without referencing either.
    static bool HasAttribute(ImmutableArray<AttributeData> attributes, string name, bool inherit)
    {
        foreach (var attribute in attributes)
        {
            for (var current = attribute.AttributeClass; current is not null; current = inherit ? current.BaseType : null)
            {
                if (current.Name == name)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
