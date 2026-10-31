// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;
using Microsoft.CodeAnalysis;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_FactShouldNotCallStateChangingMembersOnSharedContextAnalyzer.when_analyzing;

public class and_fact_consumes_shared_state : given.a_fact_should_not_call_state_changing_members_analyzer
{
    const string Usage = """
    public class when_something : Cratis.Specifications.Specification
    {
        Store _store;

        [Xunit.Fact]
        void should_consume()
        {
            var taken = {|#0:_store.Take()|};
            var dequeued = {|#1:_store.Dequeue()|};
            var popped = {|#2:_store.Pop()|};
            if ({|#3:_store.MoveNext()|})
            {
            }
        }
    }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<FactShouldNotCallStateChangingMembersOnSharedContextAnalyzer>.VerifyAnalyzerOnCompilingSource(
        SpecSource.Wrap(Usage) + SupportSource,
        new ExpectedDiagnostic(DiagnosticIds.FactShouldNotCallStateChangingMembersOnSharedContext, DiagnosticSeverity.Warning, "should_consume", "'Take'", "'_store'"),
        new ExpectedDiagnostic(DiagnosticIds.FactShouldNotCallStateChangingMembersOnSharedContext, DiagnosticSeverity.Warning, "should_consume", "'Dequeue'", "'_store'"),
        new ExpectedDiagnostic(DiagnosticIds.FactShouldNotCallStateChangingMembersOnSharedContext, DiagnosticSeverity.Warning, "should_consume", "'Pop'", "'_store'"),
        new ExpectedDiagnostic(DiagnosticIds.FactShouldNotCallStateChangingMembersOnSharedContext, DiagnosticSeverity.Warning, "should_consume", "'MoveNext'", "'_store'"));

    [Fact] Task should_report_each_consuming_read() => _result;
}
