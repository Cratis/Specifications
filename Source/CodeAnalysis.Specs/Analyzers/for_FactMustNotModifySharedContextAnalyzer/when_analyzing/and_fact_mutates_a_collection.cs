// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;
using Microsoft.CodeAnalysis;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_FactMustNotModifySharedContextAnalyzer.when_analyzing;

public class and_fact_mutates_a_collection : given.a_fact_must_not_modify_shared_context_analyzer
{
    const string Usage = """
    public class when_something : Cratis.Specifications.Specification
    {
        System.Collections.Generic.List<int> _items;

        [Xunit.Fact]
        void should_change()
        {
            {|#0:_items.Add(1)|};
            {|#1:_items.Clear()|};
        }
    }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<FactMustNotModifySharedContextAnalyzer>.VerifyAnalyzer(
        SpecSource.Wrap(Usage, "Sample.when_something"),
        new ExpectedDiagnostic(DiagnosticIds.FactMustNotModifySharedContext, DiagnosticSeverity.Info, "should_change", "_items"),
        new ExpectedDiagnostic(DiagnosticIds.FactMustNotModifySharedContext, DiagnosticSeverity.Info, "should_change", "_items"));

    [Fact] Task should_report_each_mutating_call() => _result;
}
