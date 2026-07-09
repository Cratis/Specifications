// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_FactOnGivenBaseClassAnalyzer;

public class when_creating_analyzer : Specification
{
    FactOnGivenBaseClassAnalyzer _analyzer;

    void Establish() => _analyzer = new FactOnGivenBaseClassAnalyzer();

    [Fact] void should_have_supported_diagnostics() => _analyzer.SupportedDiagnostics.ShouldNotBeEmpty();
    [Fact] void should_support_crspec0003() => _analyzer.SupportedDiagnostics.Any(d => d.Id == DiagnosticIds.FactOnGivenBaseClass).ShouldBeTrue();
}
