// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;
using Microsoft.CodeAnalysis;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_FactShouldNotCallStateChangingMembersOnSharedContextAnalyzer.when_analyzing;

public class and_fact_calls_through_a_null_conditional_access : given.a_fact_should_not_call_state_changing_members_analyzer
{
    const string Usage = """
    public class when_something : Cratis.Specifications.Specification
    {
        Store _store;
        Owner _owner;

        [Xunit.Fact]
        void should_change()
        {
            {|#0:_store?.Dequeue()|};
            {|#1:_owner?.Items.Clear()|};
            {|#2:_owner.Store?.Enqueue(1)|};
        }
    }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<FactShouldNotCallStateChangingMembersOnSharedContextAnalyzer>.VerifyAnalyzerOnCompilingSource(
        SpecSource.Wrap(Usage) + SupportSource,
        new ExpectedDiagnostic(DiagnosticIds.FactShouldNotCallStateChangingMembersOnSharedContext, DiagnosticSeverity.Warning, "should_change", "'Dequeue'", "'_store'"),
        new ExpectedDiagnostic(DiagnosticIds.FactShouldNotCallStateChangingMembersOnSharedContext, DiagnosticSeverity.Warning, "should_change", "'Clear'", "'_owner'"),
        new ExpectedDiagnostic(DiagnosticIds.FactShouldNotCallStateChangingMembersOnSharedContext, DiagnosticSeverity.Warning, "should_change", "'Enqueue'", "'_owner'"));

    [Fact] Task should_report_each_call() => _result;
}
