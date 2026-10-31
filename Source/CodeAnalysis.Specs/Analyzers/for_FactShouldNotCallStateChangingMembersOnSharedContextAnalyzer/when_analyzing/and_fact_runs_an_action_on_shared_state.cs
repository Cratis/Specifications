// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;
using Microsoft.CodeAnalysis;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_FactShouldNotCallStateChangingMembersOnSharedContextAnalyzer.when_analyzing;

public class and_fact_runs_an_action_on_shared_state : given.a_fact_should_not_call_state_changing_members_analyzer
{
    const string Usage = """
    public class when_something : Cratis.Specifications.Specification
    {
        Scenario _scenario;
        Handler _handler;

        [Xunit.Fact]
        async System.Threading.Tasks.Task should_run()
        {
            await {|#0:_scenario.Execute(1)|};
            {|#1:this._handler.Handle(2)|};
        }
    }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<FactShouldNotCallStateChangingMembersOnSharedContextAnalyzer>.VerifyAnalyzerOnCompilingSource(
        SpecSource.Wrap(Usage) + SupportSource,
        new ExpectedDiagnostic(DiagnosticIds.FactShouldNotCallStateChangingMembersOnSharedContext, DiagnosticSeverity.Warning, "should_run", "'Execute'", "'_scenario'", "Because", "[PerFactSpecificationLifecycle]"),
        new ExpectedDiagnostic(DiagnosticIds.FactShouldNotCallStateChangingMembersOnSharedContext, DiagnosticSeverity.Warning, "should_run", "'Handle'", "'_handler'"));

    [Fact] Task should_report_each_action() => _result;
}
