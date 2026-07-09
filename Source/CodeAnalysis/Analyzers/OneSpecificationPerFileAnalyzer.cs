// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Cratis.Specifications.CodeAnalysis.Analyzers;

/// <summary>
/// Analyzer that warns when a single file declares more than one Specification-derived class.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class OneSpecificationPerFileAnalyzer : DiagnosticAnalyzer
{
    static readonly DiagnosticDescriptor _rule = new(
        id: DiagnosticIds.OneSpecificationPerFile,
        title: "A file should declare at most one specification",
        messageFormat: "Specification '{0}' should be moved to its own file; a file should declare at most one specification",
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Cratis specifications map one behavior to one file (for_/when_/and_). Declaring more than one Specification-derived class in a single file hides scenarios and breaks the navigable one-file-per-behavior convention. Shared context types in a 'given' namespace and '*SpecHelpers' types are exempt.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSemanticModelAction(AnalyzeSemanticModel);
    }

    static void AnalyzeSemanticModel(SemanticModelAnalysisContext context)
    {
        var root = context.SemanticModel.SyntaxTree.GetRoot(context.CancellationToken);
        var specifications = new List<(INamedTypeSymbol Symbol, ClassDeclarationSyntax Declaration)>();

        foreach (var declaration in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
        {
            if (!IsTopLevel(declaration))
            {
                continue;
            }

            if (context.SemanticModel.GetDeclaredSymbol(declaration, context.CancellationToken) is not { } symbol)
            {
                continue;
            }

            if (!SpecificationFacts.DerivesFromSpecification(symbol) || IsExempt(symbol))
            {
                continue;
            }

            specifications.Add((symbol, declaration));
        }

        if (specifications.Count <= 1)
        {
            return;
        }

        // The first specification stays; every additional one should move to its own file.
        foreach (var (symbol, declaration) in specifications.Skip(1))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                _rule,
                declaration.Identifier.GetLocation(),
                symbol.Name));
        }
    }

    static bool IsTopLevel(ClassDeclarationSyntax declaration) =>
        declaration.Parent is BaseNamespaceDeclarationSyntax or CompilationUnitSyntax;

    static bool IsExempt(INamedTypeSymbol symbol) =>
        symbol.Name.EndsWith(SpecificationFacts.SpecHelpersSuffix, System.StringComparison.Ordinal) ||
        SpecificationFacts.IsInGivenNamespace(symbol);
}
