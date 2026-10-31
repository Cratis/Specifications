// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;
using Microsoft.CodeAnalysis;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_FactMustNotModifySharedContextAnalyzer.when_analyzing;

public class and_fact_reconfigures_a_shared_substitute : given.a_fact_must_not_modify_shared_context_analyzer
{
    const string Usage = """
    using NSubstitute;

    public interface IService
    {
        int Get();
        void Run();
    }

    public class when_something : Cratis.Specifications.Specification
    {
        IService _service;

        [Xunit.Fact]
        void should_reconfigure()
        {
            {|#0:_service.Get().Returns(1)|};
            {|#1:_service.When(_ => _.Run())|}.Do(_ => { });
            {|#2:_service.ClearReceivedCalls()|};
        }
    }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<FactMustNotModifySharedContextAnalyzer>.VerifyAnalyzerOnCompilingSource(
        SpecSource.Wrap(Usage) + SubstituteSource,
        new ExpectedDiagnostic(DiagnosticIds.FactMustNotModifySharedContext, DiagnosticSeverity.Error, "should_reconfigure", "'_service'"),
        new ExpectedDiagnostic(DiagnosticIds.FactMustNotModifySharedContext, DiagnosticSeverity.Error, "should_reconfigure", "'_service'"),
        new ExpectedDiagnostic(DiagnosticIds.FactMustNotModifySharedContext, DiagnosticSeverity.Error, "should_reconfigure", "'_service'"));

    [Fact] Task should_report_each_reconfiguration() => _result;
}
