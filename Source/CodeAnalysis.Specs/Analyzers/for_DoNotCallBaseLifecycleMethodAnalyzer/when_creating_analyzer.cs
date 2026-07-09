// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_DoNotCallBaseLifecycleMethodAnalyzer;

public class when_creating_analyzer : Specification
{
    DoNotCallBaseLifecycleMethodAnalyzer _analyzer;

    void Establish() => _analyzer = new DoNotCallBaseLifecycleMethodAnalyzer();

    [Fact] void should_have_supported_diagnostics() => _analyzer.SupportedDiagnostics.ShouldNotBeEmpty();
    [Fact] void should_support_crspec0005() => _analyzer.SupportedDiagnostics.Any(d => d.Id == DiagnosticIds.DoNotCallBaseLifecycleMethod).ShouldBeTrue();
}
