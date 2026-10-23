// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Cratis.Specifications.CodeAnalysis.Analyzers;

/// <summary>
/// Rejects two lifecycle names for the same phase on a single declaring type.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AmbiguousLifecycleAliasesAnalyzer : DiagnosticAnalyzer
{
    static readonly DiagnosticDescriptor _rule = new(
        DiagnosticIds.AmbiguousLifecycleAliases,
        "Use one lifecycle name per phase",
        "'{0}' declares both {1} and {2}; use one of them",
        "Reliability", DiagnosticSeverity.Error, isEnabledByDefault: true);

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [_rule];

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
        if (!SpecificationFacts.DerivesFromSpecification(type)) return;
        Check(context, type, "Establish", "Given");
        Check(context, type, "Because", "When");
    }

    static void Check(SymbolAnalysisContext context, INamedTypeSymbol type, string primary, string alias)
    {
        var methods = type.GetMembers().OfType<IMethodSymbol>()
            .Where(method => !method.IsStatic && !method.IsGenericMethod && !method.IsOverride && method.Parameters.Length == 0).ToArray();
        if (methods.Any(method => method.Name == primary) && methods.FirstOrDefault(method => method.Name == alias) is IMethodSymbol duplicate)
        {
            context.ReportDiagnostic(Diagnostic.Create(_rule, duplicate.Locations[0], type.Name, primary, alias));
        }
    }
}
