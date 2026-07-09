// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_FactMethodShouldNamingAnalyzer.when_analyzing;

public class and_class_is_not_a_specification : given.a_fact_method_should_naming_analyzer
{
    const string Usage = """
    public class PlainTestClass
    {
        [Xunit.Fact] void it_works() { }
    }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<FactMethodShouldNamingAnalyzer>.VerifyAnalyzer(SpecSource.Wrap(Usage));

    [Fact] Task should_not_report_any_diagnostics() => _result;
}
