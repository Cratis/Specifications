// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;
using Microsoft.CodeAnalysis;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_ShouldMethodMustBeTestMethodAnalyzer.when_analyzing;

public class and_should_method_has_no_test_attribute : given.a_should_method_must_be_test_method_analyzer
{
    const string Usage = """
    public class when_doing_something : Cratis.Specifications.Specification
    {
        void {|#0:should_do_something|}() { }
    }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<ShouldMethodMustBeTestMethodAnalyzer>.VerifyAnalyzer(
        SpecSource.Wrap(Usage),
        new ExpectedDiagnostic(DiagnosticIds.ShouldMethodMustBeTestMethod, DiagnosticSeverity.Warning, "should_do_something"));

    [Fact] Task should_report_the_missing_test_attribute() => _result;
}
