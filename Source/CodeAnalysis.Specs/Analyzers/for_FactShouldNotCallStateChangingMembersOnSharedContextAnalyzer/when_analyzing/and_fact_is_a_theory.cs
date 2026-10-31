// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;
using Microsoft.CodeAnalysis;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_FactShouldNotCallStateChangingMembersOnSharedContextAnalyzer.when_analyzing;

public class and_fact_is_a_theory : given.a_fact_should_not_call_state_changing_members_analyzer
{
    const string Usage = """
    public class when_something : Cratis.Specifications.Specification
    {
        Store _store;

        [Xunit.Theory]
        void should_consume() => _store.Dequeue();

        [Xunit.Fact]
        void should_consume_once() => {|#0:_store.Pop()|};
    }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<FactShouldNotCallStateChangingMembersOnSharedContextAnalyzer>.VerifyAnalyzerOnCompilingSource(
        SpecSource.Wrap(Usage) + SupportSource,
        new ExpectedDiagnostic(DiagnosticIds.FactShouldNotCallStateChangingMembersOnSharedContext, DiagnosticSeverity.Warning, "should_consume_once", "'Pop'"));

    [Fact] Task should_report_only_the_fact() => _result;
}
