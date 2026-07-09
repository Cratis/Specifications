// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Cratis.Specifications.CodeAnalysis.Analyzers;

/// <summary>
/// Analyzer that warns when a specification declaring test methods is not publicly accessible.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SpecificationMustBePublicAnalyzer : DiagnosticAnalyzer
{
    static readonly DiagnosticDescriptor _rule = new(
        id: DiagnosticIds.SpecificationMustBePublic,
        title: "Specifications with test methods must be public",
        messageFormat: "Specification '{0}' declares test methods but is not public, so the runner never discovers it",
        category: "Reliability",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The test runner only discovers publicly accessible test classes. A Specification-derived class that declares test methods but is internal (or nested inside a non-public type) is silently skipped, producing a false sense of coverage. Make the specification (and any type containing it) public.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
    }

    static void AnalyzeType(SymbolAnalysisContext context)
    {
        var type = (INamedTypeSymbol)context.Symbol;

        if (type.TypeKind != TypeKind.Class || type.IsAbstract)
        {
            return;
        }

        if (!SpecificationFacts.DerivesFromSpecification(type))
        {
            return;
        }

        if (SpecificationFacts.IsEffectivelyPublic(type))
        {
            return;
        }

        if (!SpecificationFacts.HasTestMethod(type))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            _rule,
            type.Locations.FirstOrDefault(),
            type.Name));
    }
}
