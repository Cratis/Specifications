// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Cratis.Specifications.CodeAnalysis.Analyzers;

/// <summary>
/// Analyzer that warns when a test method inside a specification is not named 'should_*'.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class FactMethodShouldNamingAnalyzer : DiagnosticAnalyzer
{
    const string ShouldPrefix = "should_";

    static readonly DiagnosticDescriptor _rule = new(
        id: DiagnosticIds.FactMethodMustBeNamedShould,
        title: "Specification test methods should be named 'should_*'",
        messageFormat: "Test method '{0}' should be named 'should_*' to read as a behavioral fact",
        category: "Naming",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Cratis specifications read as behavioral facts (Given/When/Then). A test method ([Fact], [Theory], [Test], or [TestCase]) inside a Specification-derived class should start with 'should_' so the test output reads as a sentence, for example 'should_reject_the_command'. This keeps the BDD readability that is the whole point of the framework.");

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

        if (!SpecificationFacts.DerivesFromSpecification(method.ContainingType))
        {
            return;
        }

        if (!SpecificationFacts.IsTestMethod(method))
        {
            return;
        }

        if (method.Name.StartsWith(ShouldPrefix, StringComparison.Ordinal))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            _rule,
            method.Locations.FirstOrDefault(),
            method.Name));
    }
}
