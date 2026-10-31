// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_FactShouldNotCallStateChangingMembersOnSharedContextAnalyzer.when_analyzing;

public class and_fact_verifies_or_configures_a_substitute : given.a_fact_should_not_call_state_changing_members_analyzer
{
    const string Usage = """
    using NSubstitute;

    public class when_something : Cratis.Specifications.Specification
    {
        Scenario _scenario;
        Handler _handler;
        System.Threading.Tasks.Task _task;

        [Xunit.Fact]
        void should_verify()
        {
            _scenario.Received().Execute(1);
            _scenario.DidNotReceive().Execute(1);
            _scenario.ReceivedWithAnyArgs().Execute(1);
            _scenario.Execute(1).Returns(_task);
            SubstituteExtensions.Returns(_scenario.Execute(2), _task);
            _handler.When(_ => _.Handle(1));
        }
    }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<FactShouldNotCallStateChangingMembersOnSharedContextAnalyzer>.VerifyAnalyzerOnCompilingSource(
        SpecSource.Wrap(Usage) + SupportSource);

    [Fact] Task should_not_report_any_diagnostics() => _result;
}
