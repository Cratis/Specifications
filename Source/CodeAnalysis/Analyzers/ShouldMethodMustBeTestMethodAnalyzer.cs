// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Cratis.Specifications.CodeAnalysis.Analyzers;

/// <summary>
/// Analyzer that warns when a 'should_*' method inside a specification is missing a test attribute.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ShouldMethodMustBeTestMethodAnalyzer : DiagnosticAnalyzer
{
    static readonly DiagnosticDescriptor _rule = new(
        id: DiagnosticIds.ShouldMethodMustBeTestMethod,
        title: "'should_*' methods must be test methods",
        messageFormat: "Method '{0}' is named like a fact but has no test attribute, so it never runs; add [Fact] (or [Theory]/[Test])",
        category: "Reliability",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A method named 'should_*' inside a Specification-derived class reads as a behavioral fact, but without a test attribute ([Fact], [Theory], [Test], or [TestCase]) it is just an ordinary private method that the runner never executes. This silently drops the assertion and creates a false sense of coverage. Add the missing test attribute, or rename the method if it is a helper.");

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

        if (method.MethodKind != MethodKind.Ordinary || method.IsStatic || method.IsOverride || method.IsAbstract)
        {
            return;
        }

        if (!method.Name.StartsWith(SpecificationFacts.ShouldPrefix, StringComparison.Ordinal))
        {
            return;
        }

        if (!SpecificationFacts.DerivesFromSpecification(method.ContainingType))
        {
            return;
        }

        if (SpecificationFacts.IsTestMethod(method))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            _rule,
            method.Locations.FirstOrDefault(),
            method.Name));
    }
}
