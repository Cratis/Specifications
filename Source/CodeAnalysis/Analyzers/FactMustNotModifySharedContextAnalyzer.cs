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
/// Analyzer that reports when a fact modifies state shared through the specification instance.
/// </summary>
/// <remarks>
/// Reported as an error, because specifications share one context instance by default. Specifications marked
/// with <c>PerFactSpecificationLifecycle</c> (on the class, a containing or base class, or the assembly) and
/// theories are not analyzed. Lower the severity through <c>dotnet_diagnostic.CRSPEC0008.severity</c> in an .editorconfig.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class FactMustNotModifySharedContextAnalyzer : DiagnosticAnalyzer
{
    static readonly HashSet<string> _mutatingMethodNames = new(System.StringComparer.Ordinal)
    {
        "Add",
        "AddRange",
        "Remove",
        "RemoveAt",
        "Clear",
        "Insert"
    };

    const string PerFactAttributeName = "PerFactSpecificationLifecycleAttribute";
    const string TheoryAttributeName = "TheoryAttribute";
    const string ImmutableCollectionsNamespace = "System.Collections.Immutable";

    static readonly DiagnosticDescriptor _rule = new(
        id: DiagnosticIds.FactMustNotModifySharedContext,
        title: "Facts should not modify the shared specification context",
        messageFormat: "Fact '{0}' modifies the shared specification state '{1}'; establish state in Establish/Because so facts stay independent of execution order",
        category: "Reliability",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "All facts of a specification share one context instance that is established once. A fact that assigns to, or mutates, a field or property of that context makes other facts depend on execution order. Move the change into Establish or Because, or use a local variable.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [_rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(compilationContext =>
        {
            if (compilationContext.Compilation.GetTypeByMetadataName(SpecificationFacts.SpecificationBaseTypeName) is null)
            {
                return;
            }

            if (HasPerFactAttribute(compilationContext.Compilation.Assembly.GetAttributes()))
            {
                return;
            }

            compilationContext.RegisterSyntaxNodeAction(
                AnalyzeAssignment,
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
                SyntaxKind.CoalesceAssignmentExpression);
            compilationContext.RegisterSyntaxNodeAction(
                AnalyzeUnary,
                SyntaxKind.PreIncrementExpression,
                SyntaxKind.PostIncrementExpression,
                SyntaxKind.PreDecrementExpression,
                SyntaxKind.PostDecrementExpression);
            compilationContext.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        });
    }

    static void AnalyzeAssignment(SyntaxNodeAnalysisContext context) =>
        Analyze(context, ((AssignmentExpressionSyntax)context.Node).Left);

    static void AnalyzeUnary(SyntaxNodeAnalysisContext context) =>
        Analyze(context, context.Node switch
        {
            PrefixUnaryExpressionSyntax prefix => prefix.Operand,
            PostfixUnaryExpressionSyntax postfix => postfix.Operand,
            _ => null
        });

    static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess ||
            !_mutatingMethodNames.Contains(memberAccess.Name.Identifier.ValueText) ||
            !IsDiscardedResult(invocation))
        {
            return;
        }

        // Immutable collections return a new collection and leave the receiver untouched.
        if (context.SemanticModel.GetTypeInfo(memberAccess.Expression, context.CancellationToken).Type is { } receiverType &&
            IsImmutableCollection(receiverType))
        {
            return;
        }

        AnalyzeUnlessVerifying(context, memberAccess.Expression);
    }

    static void AnalyzeUnlessVerifying(SyntaxNodeAnalysisContext context, ExpressionSyntax? target)
    {
        if (!IsInsideReceivedInOrder(context))
        {
            Analyze(context, target);
        }
    }

    static void Analyze(SyntaxNodeAnalysisContext context, ExpressionSyntax? target)
    {
        var name = GetMemberName(target);
        if (name is null ||
            context.ContainingSymbol is not IMethodSymbol { MethodKind: MethodKind.Ordinary } method ||
            !SpecificationFacts.DerivesFromSpecification(method.ContainingType) ||
            !SpecificationFacts.IsTestMethod(method) ||
            IsTheory(method) ||
            IsPerFact(method.ContainingType))
        {
            return;
        }

        var symbol = context.SemanticModel.GetSymbolInfo(target!, context.CancellationToken).Symbol;
        if (symbol is not (IFieldSymbol or IPropertySymbol) || symbol.IsStatic || !IsMemberOfSpecification(symbol, method.ContainingType))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(_rule, context.Node.GetLocation(), method.Name, name));
    }

    static bool IsInsideReceivedInOrder(SyntaxNodeAnalysisContext context)
    {
        for (var ancestor = context.Node.Parent; ancestor is not null and not MethodDeclarationSyntax; ancestor = ancestor.Parent)
        {
            if (ancestor is AnonymousFunctionExpressionSyntax
                {
                    Parent: ArgumentSyntax { Parent: ArgumentListSyntax { Parent: InvocationExpressionSyntax invocation } }
                } && IsReceivedInOrder(context, invocation))
            {
                return true;
            }
        }

        return false;
    }

    static bool IsReceivedInOrder(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation)
    {
        if (context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol is not IMethodSymbol { Name: "InOrder" } method)
        {
            return false;
        }

        return SymbolEqualityComparer.Default.Equals(method.ContainingType, context.Compilation.GetTypeByMetadataName("NSubstitute.Received"));
    }

    // A mutating call whose result is used (fluent chain, assignment, argument) is not treated as a mutation of the field.
    static bool IsDiscardedResult(InvocationExpressionSyntax invocation) =>
        invocation.Parent is ExpressionStatementSyntax or ArrowExpressionClauseSyntax;

    static bool IsImmutableCollection(ITypeSymbol type) =>
        type.ContainingNamespace?.ToDisplayString() == ImmutableCollectionsNamespace;

    static bool IsTheory(IMethodSymbol method) => HasAttribute(method.GetAttributes(), TheoryAttributeName, inherit: true);

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

    // Only plain 'member' or 'this.member' targets qualify; anything else (indexers, other instances) is ignored to stay conservative.
    static string? GetMemberName(ExpressionSyntax? target) => target switch
    {
        IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
        MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax } access => access.Name.Identifier.ValueText,
        _ => null
    };

    static bool IsMemberOfSpecification(ISymbol member, INamedTypeSymbol specification)
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
}
