// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;
using Microsoft.CodeAnalysis;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_FactMustNotModifySharedContextAnalyzer.when_analyzing;

public class and_fact_records_a_call_outside_in_order : given.a_fact_must_not_modify_shared_context_analyzer
{
    const string Usage = """
    public class when_something : Cratis.Specifications.Specification
    {
        System.Collections.Generic.ICollection<string> _store;

        [Xunit.Fact]
        void should_remove()
        {
            NSubstitute.Received.InOrder(() => { _store.Remove("x"); });
            {|#0:_store.Remove("y")|};
        }
    }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<FactMustNotModifySharedContextAnalyzer>.VerifyAnalyzer(
        SpecSource.Wrap(Usage) + InOrderSource,
        new ExpectedDiagnostic(DiagnosticIds.FactMustNotModifySharedContext, DiagnosticSeverity.Error, "should_remove", "_store"));

    [Fact] Task should_report_the_call_outside_in_order() => _result;
}
