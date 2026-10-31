// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cratis.Specifications.CodeAnalysis.Analyzers;

/// <summary>
/// A call of a named member on a receiver, written either as <c>receiver.Name()</c> or as <c>receiver?.Name()</c>.
/// </summary>
/// <param name="receiver">The expression the member is called on.</param>
/// <param name="name">The name of the called member.</param>
sealed class MemberCall(ExpressionSyntax receiver, string name)
{
    /// <summary>
    /// Gets the expression the member is called on.
    /// </summary>
    public ExpressionSyntax Receiver { get; } = receiver;

    /// <summary>
    /// Gets the name of the called member.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Gets the member call an invocation makes.
    /// </summary>
    /// <param name="invocation">The invocation to inspect.</param>
    /// <returns>The member call, or null if the invocation does not call a member on a receiver.</returns>
    public static MemberCall? From(InvocationExpressionSyntax invocation) => invocation.Expression switch
    {
        MemberAccessExpressionSyntax memberAccess => new MemberCall(memberAccess.Expression, memberAccess.Name.Identifier.ValueText),
        MemberBindingExpressionSyntax binding when ConditionalReceiverOf(binding) is { } receiver => new MemberCall(receiver, binding.Name.Identifier.ValueText),
        _ => null
    };

    /// <summary>
    /// Gets the receiver a member or element binding (<c>?.Name</c>, <c>?[key]</c>) is evaluated on.
    /// </summary>
    /// <param name="binding">The binding expression.</param>
    /// <returns>The expression before the <c>?</c>, or null if the binding is not inside a conditional access.</returns>
    public static ExpressionSyntax? ConditionalReceiverOf(SyntaxNode binding)
    {
        var child = binding;
        for (var current = binding.Parent; current is not null; child = current, current = current.Parent)
        {
            if (current is ConditionalAccessExpressionSyntax conditionalAccess && conditionalAccess.WhenNotNull == child)
            {
                return conditionalAccess.Expression;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the expression that carries the result of an expression, stepping out of the conditional accesses it ends,
    /// so that <c>.Clear()</c> in <c>_x?.Clear()</c> gives the whole <c>_x?.Clear()</c>.
    /// </summary>
    /// <param name="expression">The expression, such as an invocation or an assignment.</param>
    /// <returns>The outermost conditional access the expression ends, or the expression itself.</returns>
    public static ExpressionSyntax ResultOf(ExpressionSyntax expression)
    {
        var result = expression;
        while (result.Parent is ConditionalAccessExpressionSyntax conditionalAccess && conditionalAccess.WhenNotNull == result)
        {
            result = conditionalAccess;
        }

        return result;
    }
}
