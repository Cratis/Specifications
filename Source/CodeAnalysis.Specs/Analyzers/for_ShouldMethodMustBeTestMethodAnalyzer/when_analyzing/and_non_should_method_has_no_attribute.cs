// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_ShouldMethodMustBeTestMethodAnalyzer.when_analyzing;

public class and_non_should_method_has_no_attribute : given.a_should_method_must_be_test_method_analyzer
{
    const string Usage = """
    public class when_doing_something : Cratis.Specifications.Specification
    {
        void Establish() { }
        void a_helper() { }
    }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<ShouldMethodMustBeTestMethodAnalyzer>.VerifyAnalyzer(SpecSource.Wrap(Usage));

    [Fact] Task should_not_report_any_diagnostics() => _result;
}
