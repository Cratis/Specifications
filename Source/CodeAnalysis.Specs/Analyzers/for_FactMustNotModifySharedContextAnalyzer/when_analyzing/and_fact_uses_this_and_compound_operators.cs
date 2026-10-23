// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;
using Microsoft.CodeAnalysis;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_FactMustNotModifySharedContextAnalyzer.when_analyzing;

public class and_fact_uses_this_and_compound_operators : given.a_fact_must_not_modify_shared_context_analyzer
{
    const string Usage = """
    public class when_something : Cratis.Specifications.Specification
    {
        int _value;
        int Count { get; set; }

        [Xunit.Fact]
        void should_change()
        {
            {|#0:this._value = 1|};
            {|#1:_value += 2|};
            {|#2:Count++|};
            {|#3:--_value|};
        }
    }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<FactMustNotModifySharedContextAnalyzer>.VerifyAnalyzer(
        SpecSource.Wrap(Usage, "Sample.when_something"),
        new ExpectedDiagnostic(DiagnosticIds.FactMustNotModifySharedContext, DiagnosticSeverity.Info, "should_change", "_value"),
        new ExpectedDiagnostic(DiagnosticIds.FactMustNotModifySharedContext, DiagnosticSeverity.Info, "should_change", "_value"),
        new ExpectedDiagnostic(DiagnosticIds.FactMustNotModifySharedContext, DiagnosticSeverity.Info, "should_change", "Count"),
        new ExpectedDiagnostic(DiagnosticIds.FactMustNotModifySharedContext, DiagnosticSeverity.Info, "should_change", "_value"));

    [Fact] Task should_report_every_modification() => _result;
}
