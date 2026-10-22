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
/// Reported as info by default so existing code bases are not broken; raise the severity through
/// <c>dotnet_diagnostic.CRSPEC0008.severity</c> in an .editorconfig.
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

    static readonly DiagnosticDescriptor _rule = new(
        id: DiagnosticIds.FactMustNotModifySharedContext,
        title: "Facts should not modify the shared specification context",
        messageFormat: "Fact '{0}' modifies the shared specification state '{1}'; establish state in Establish/Because so facts stay independent of execution order",
        category: "Reliability",
        defaultSeverity: DiagnosticSeverity.Info,
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
            !_mutatingMethodNames.Contains(memberAccess.Name.Identifier.ValueText))
        {
            return;
        }

        Analyze(context, memberAccess.Expression);
    }

    static void Analyze(SyntaxNodeAnalysisContext context, ExpressionSyntax? target)
    {
        var name = GetMemberName(target);
        if (name is null ||
            context.ContainingSymbol is not IMethodSymbol { MethodKind: MethodKind.Ordinary } method ||
            !SpecificationFacts.DerivesFromSpecification(method.ContainingType) ||
            !SpecificationFacts.IsTestMethod(method))
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
