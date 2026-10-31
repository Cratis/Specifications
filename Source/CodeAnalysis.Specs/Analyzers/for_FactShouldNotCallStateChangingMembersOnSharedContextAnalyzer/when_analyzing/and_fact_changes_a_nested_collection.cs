// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;
using Microsoft.CodeAnalysis;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_FactShouldNotCallStateChangingMembersOnSharedContextAnalyzer.when_analyzing;

public class and_fact_changes_a_nested_collection : given.a_fact_should_not_call_state_changing_members_analyzer
{
    const string Usage = """
    public class when_something : Cratis.Specifications.Specification
    {
        Owner _owner;
        System.Collections.Generic.List<int>[] _lists;
        Store _store;

        [Xunit.Fact]
        void should_change()
        {
            {|#0:_owner.Items.Add(1)|};
            {|#1:_owner.Items.Clear()|};
            {|#2:_lists[0].Remove(1)|};
            {|#3:_owner.Store.Enqueue(1)|};
            if ({|#4:_store.TryAdd(1)|})
            {
            }

            {|#5:_store.Reset()|};
        }
    }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<FactShouldNotCallStateChangingMembersOnSharedContextAnalyzer>.VerifyAnalyzerOnCompilingSource(
        SpecSource.Wrap(Usage) + SupportSource,
        new ExpectedDiagnostic(DiagnosticIds.FactShouldNotCallStateChangingMembersOnSharedContext, DiagnosticSeverity.Warning, "should_change", "'Add'", "'_owner'"),
        new ExpectedDiagnostic(DiagnosticIds.FactShouldNotCallStateChangingMembersOnSharedContext, DiagnosticSeverity.Warning, "should_change", "'Clear'", "'_owner'"),
        new ExpectedDiagnostic(DiagnosticIds.FactShouldNotCallStateChangingMembersOnSharedContext, DiagnosticSeverity.Warning, "should_change", "'Remove'", "'_lists'"),
        new ExpectedDiagnostic(DiagnosticIds.FactShouldNotCallStateChangingMembersOnSharedContext, DiagnosticSeverity.Warning, "should_change", "'Enqueue'", "'_owner'"),
        new ExpectedDiagnostic(DiagnosticIds.FactShouldNotCallStateChangingMembersOnSharedContext, DiagnosticSeverity.Warning, "should_change", "'TryAdd'", "'_store'"),
        new ExpectedDiagnostic(DiagnosticIds.FactShouldNotCallStateChangingMembersOnSharedContext, DiagnosticSeverity.Warning, "should_change", "'Reset'", "'_store'"));

    [Fact] Task should_report_each_change() => _result;
}
