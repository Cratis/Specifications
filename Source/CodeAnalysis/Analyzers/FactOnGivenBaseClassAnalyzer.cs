// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Cratis.Specifications.CodeAnalysis.Analyzers;

/// <summary>
/// Analyzer that warns when a test method is declared on a specification context in a 'given' namespace.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class FactOnGivenBaseClassAnalyzer : DiagnosticAnalyzer
{
    static readonly DiagnosticDescriptor _rule = new(
        id: DiagnosticIds.FactOnGivenBaseClass,
        title: "Test methods must not be declared on a 'given' context",
        messageFormat: "Test method '{0}' is declared on the '{1}' context; move it to a 'when_' specification so it actually runs",
        category: "Reliability",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A 'given' type establishes a reusable context (Given) and is inherited by 'when_' specifications; it is not itself a runnable specification. A test method ([Fact], [Theory], [Test], or [TestCase]) placed on a 'given' context is silently never executed, producing a real coverage gap. Move the assertion into a 'when_' specification that derives from the context.");

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

        if (method.MethodKind != MethodKind.Ordinary)
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

        if (!SpecificationFacts.IsTestMethod(method))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            _rule,
            method.Locations.FirstOrDefault(),
            method.Name,
            containingType.Name));
    }
}
