// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;
using Microsoft.CodeAnalysis;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_FactMustNotModifySharedContextAnalyzer.when_analyzing;

public class and_fact_assigns_a_field : given.a_fact_must_not_modify_shared_context_analyzer
{
    const string Usage = """
    public class when_something : Cratis.Specifications.Specification
    {
        int _value;

        [Xunit.Fact] void should_change() => {|#0:_value = 1|};
    }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<FactMustNotModifySharedContextAnalyzer>.VerifyAnalyzer(
        SpecSource.Wrap(Usage, "Sample.when_something"),
        new ExpectedDiagnostic(DiagnosticIds.FactMustNotModifySharedContext, DiagnosticSeverity.Error, "should_change", "_value"));

    [Fact] Task should_report_the_assignment() => _result;
}
