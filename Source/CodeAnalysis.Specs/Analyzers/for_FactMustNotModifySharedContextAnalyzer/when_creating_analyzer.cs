// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_FactMustNotModifySharedContextAnalyzer;

public class when_creating_analyzer : Specification
{
    FactMustNotModifySharedContextAnalyzer _analyzer;

    void Establish() => _analyzer = new FactMustNotModifySharedContextAnalyzer();

    [Fact] void should_have_supported_diagnostics() => _analyzer.SupportedDiagnostics.ShouldNotBeEmpty();
    [Fact] void should_support_crspec0008() => _analyzer.SupportedDiagnostics.Any(d => d.Id == DiagnosticIds.FactMustNotModifySharedContext).ShouldBeTrue();
    [Fact] void should_be_info_by_default() => _analyzer.SupportedDiagnostics[0].DefaultSeverity.ShouldEqual(Microsoft.CodeAnalysis.DiagnosticSeverity.Info);
}
