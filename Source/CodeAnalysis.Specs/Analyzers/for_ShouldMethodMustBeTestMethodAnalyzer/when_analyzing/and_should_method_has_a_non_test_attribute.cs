// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;
using Microsoft.CodeAnalysis;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_ShouldMethodMustBeTestMethodAnalyzer.when_analyzing;

public class and_should_method_has_a_non_test_attribute : given.a_should_method_must_be_test_method_analyzer
{
    const string Usage = """
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public sealed class CustomMarkerAttribute : System.Attribute { }

    public class when_doing_something : Cratis.Specifications.Specification
    {
        [CustomMarker] void {|#0:should_do_something|}() { }
    }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<ShouldMethodMustBeTestMethodAnalyzer>.VerifyAnalyzer(
        SpecSource.Wrap(Usage),
        new ExpectedDiagnostic(DiagnosticIds.ShouldMethodMustBeTestMethod, DiagnosticSeverity.Warning, "should_do_something"));

    [Fact] Task should_report_the_missing_test_attribute() => _result;
}
