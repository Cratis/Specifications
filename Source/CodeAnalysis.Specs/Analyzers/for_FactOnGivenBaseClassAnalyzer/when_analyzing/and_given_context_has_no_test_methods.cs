// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_FactOnGivenBaseClassAnalyzer.when_analyzing;

public class and_given_context_has_no_test_methods : given.a_fact_on_given_base_class_analyzer
{
    const string Usage = """
    public class a_reusable_context : Cratis.Specifications.Specification
    {
        void Establish() { }
    }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<FactOnGivenBaseClassAnalyzer>.VerifyAnalyzer(
        SpecSource.Wrap(Usage, "Sample.for_something.given"));

    [Fact] Task should_not_report_any_diagnostics() => _result;
}
