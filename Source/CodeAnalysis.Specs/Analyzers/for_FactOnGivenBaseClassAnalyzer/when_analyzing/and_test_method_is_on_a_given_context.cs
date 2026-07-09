// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;
using Microsoft.CodeAnalysis;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_FactOnGivenBaseClassAnalyzer.when_analyzing;

public class and_test_method_is_on_a_given_context : given.a_fact_on_given_base_class_analyzer
{
    const string Usage = """
    public class a_reusable_context : Cratis.Specifications.Specification
    {
        [Xunit.Fact] void {|#0:should_never_run|}() { }
    }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<FactOnGivenBaseClassAnalyzer>.VerifyAnalyzer(
        SpecSource.Wrap(Usage, "Sample.for_something.given"),
        new ExpectedDiagnostic(DiagnosticIds.FactOnGivenBaseClass, DiagnosticSeverity.Warning, "should_never_run", "a_reusable_context"));

    [Fact] Task should_report_the_fact_on_given_diagnostic() => _result;
}
