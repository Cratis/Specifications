// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Cratis.Specifications.CodeAnalysis.Analyzers;

/// <summary>
/// Analyzer that warns when the action under test (Because) is declared on a reusable 'given' context.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BecauseMustNotBeOnGivenContextAnalyzer : DiagnosticAnalyzer
{
    const string BecauseMethodName = "Because";

    static readonly DiagnosticDescriptor _rule = new(
        id: DiagnosticIds.BecauseMustNotBeOnGivenContext,
        title: "The action under test must not live on a 'given' context",
        messageFormat: "'{0}' declares a Because() method; the action under test belongs in a 'when_' specification, not in a reusable 'given' context",
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A 'given' context captures the world before the action under test (the Given). Placing a Because() on it runs the action for every specification that derives from the context, which blurs what is being tested and can trigger the action multiple times. Keep 'given' contexts to Establish() only and move Because() into the concrete 'when_' specification.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
    }

    static void AnalyzeMethod(SymbolAnalysisContext context)
    {
        var method = (IMethodSymbol)context.Symbol;

        if (method.MethodKind != MethodKind.Ordinary || method.Name != BecauseMethodName)
        {
            return;
        }

        var containingType = method.ContainingType;

        if (!SpecificationFacts.DerivesFromSpecification(containingType))
        {
            return;
        }

        if (!SpecificationFacts.IsInGivenNamespace(containingType))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            _rule,
            method.Locations.FirstOrDefault(),
            containingType.Name));
    }
}
