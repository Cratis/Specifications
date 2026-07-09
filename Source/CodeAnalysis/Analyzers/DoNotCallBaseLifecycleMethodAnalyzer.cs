// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Cratis.Specifications.CodeAnalysis.Analyzers;

/// <summary>
/// Analyzer that warns when a specification explicitly calls a base lifecycle method.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DoNotCallBaseLifecycleMethodAnalyzer : DiagnosticAnalyzer
{
    static readonly DiagnosticDescriptor _rule = new(
        id: DiagnosticIds.DoNotCallBaseLifecycleMethod,
        title: "Do not call base lifecycle methods",
        messageFormat: "'base.{0}()' is called explicitly; the framework already runs lifecycle methods base-first, so this runs '{0}' twice",
        category: "Reliability",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Cratis specifications discover and run Establish/Because/Destroy across the whole inheritance chain automatically, base-first. Calling 'base.Establish()' (or Because/Destroy) explicitly runs that context's setup a second time, which can corrupt state or double-invoke the action under test. Remove the explicit base call.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (invocation.Expression is not MemberAccessExpressionSyntax { Expression: BaseExpressionSyntax } memberAccess)
        {
            return;
        }

        var name = memberAccess.Name.Identifier.ValueText;
        if (!SpecificationFacts.LifecycleMethodNames.Contains(name))
        {
            return;
        }

        var enclosingType = context.ContainingSymbol?.ContainingType;
        if (!SpecificationFacts.DerivesFromSpecification(enclosingType))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            _rule,
            memberAccess.Name.GetLocation(),
            name));
    }
}
