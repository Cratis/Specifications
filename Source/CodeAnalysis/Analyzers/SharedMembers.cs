// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cratis.Specifications.CodeAnalysis.Analyzers;

/// <summary>
/// Resolves expressions to the instance fields and properties a specification shares between its facts.
/// </summary>
/// <param name="model">The semantic model of the analyzed tree.</param>
/// <param name="specification">The specification whose shared members count.</param>
/// <param name="cancellationToken">The cancellation token.</param>
sealed class SharedMembers(SemanticModel model, INamedTypeSymbol specification, CancellationToken cancellationToken)
{
    /// <summary>
    /// Gets the semantic model of the analyzed tree.
    /// </summary>
    public SemanticModel Model => model;

    /// <summary>
    /// Gets the specification whose shared members count.
    /// </summary>
    public INamedTypeSymbol Specification => specification;

    /// <summary>
    /// Gets the cancellation token.
    /// </summary>
    public CancellationToken CancellationToken => cancellationToken;

    /// <summary>
    /// Determines whether the expression names a member directly: <c>member</c> or <c>this.member</c>.
    /// </summary>
    /// <param name="expression">The expression to inspect.</param>
    /// <returns>True if the expression is a direct member reference.</returns>
    public static bool IsDirect(ExpressionSyntax expression) =>
        expression is IdentifierNameSyntax or MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax };

    /// <summary>
    /// Gets the name of the shared member the expression refers to directly.
    /// </summary>
    /// <param name="expression">The expression to inspect.</param>
    /// <returns>The member name, or null if the expression is not a direct reference to a shared member.</returns>
    public string? Direct(ExpressionSyntax expression) => IsDirect(expression) ? NameOfSharedMember(expression) : null;

    /// <summary>
    /// Gets the name of the shared member the expression is reached through, such as <c>_x</c> in <c>_x.Y[0].Z</c>.
    /// </summary>
    /// <param name="expression">The expression to inspect.</param>
    /// <param name="throughInvocations">Whether the chain may pass through method calls, such as <c>_x.Get().Returns(...)</c>.</param>
    /// <returns>The member name, or null if the chain does not start at a shared member.</returns>
    public string? Root(ExpressionSyntax expression, bool throughInvocations) =>
        GetRoot(expression, throughInvocations) is { } root ? NameOfSharedMember(root) : null;

    /// <summary>
    /// Determines whether the member is declared by the specification or one of its base types.
    /// </summary>
    /// <param name="member">The member to inspect.</param>
    /// <returns>True if the member belongs to the specification hierarchy.</returns>
    public bool IsMemberOfSpecification(ISymbol member)
    {
        for (var current = specification; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, member.ContainingType.OriginalDefinition) ||
                SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, member.ContainingType.OriginalDefinition))
            {
                return true;
            }
        }

        return false;
    }

    // Only member access, element access, null-conditional bindings ('?.Name', '?[key]', followed to the expression
    // before the '?') and (optionally) invocations are followed; anything else, such as a cast or a 'new' expression,
    // ends the chain conservatively.
    static ExpressionSyntax? GetRoot(ExpressionSyntax expression, bool throughInvocations)
    {
        var current = expression;
        while (current is not null && !IsDirect(current))
        {
            current = Inner(current, throughInvocations);
        }

        return current;
    }

    static ExpressionSyntax? Inner(ExpressionSyntax expression, bool throughInvocations) => expression switch
    {
        MemberAccessExpressionSyntax memberAccess => memberAccess.Expression,
        ElementAccessExpressionSyntax elementAccess => elementAccess.Expression,
        ParenthesizedExpressionSyntax parenthesized => parenthesized.Expression,
        MemberBindingExpressionSyntax or ElementBindingExpressionSyntax => MemberCall.ConditionalReceiverOf(expression),
        InvocationExpressionSyntax invocation when throughInvocations => invocation.Expression,
        _ => null
    };

    string? NameOfSharedMember(ExpressionSyntax expression)
    {
        var symbol = model.GetSymbolInfo(expression, cancellationToken).Symbol;
        return symbol is (IFieldSymbol or IPropertySymbol) and { IsStatic: false } && IsMemberOfSpecification(symbol)
            ? symbol.Name
            : null;
    }
}
