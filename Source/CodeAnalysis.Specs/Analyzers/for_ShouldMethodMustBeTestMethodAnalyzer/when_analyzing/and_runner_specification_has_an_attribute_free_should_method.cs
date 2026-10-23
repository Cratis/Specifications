// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_ShouldMethodMustBeTestMethodAnalyzer.when_analyzing;

public class and_runner_specification_has_an_attribute_free_should_method : given.a_should_method_must_be_test_method_analyzer
{
    const string Usage = """
    public class when_doing_something : Cratis.Specifications.Specification
    {
        void should_do_something() { }
    }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<ShouldMethodMustBeTestMethodAnalyzer>.VerifyRunnerAnalyzer(SpecSource.WrapForRunner(Usage));

    [Fact] Task should_not_report_any_diagnostics() => _result;
}
