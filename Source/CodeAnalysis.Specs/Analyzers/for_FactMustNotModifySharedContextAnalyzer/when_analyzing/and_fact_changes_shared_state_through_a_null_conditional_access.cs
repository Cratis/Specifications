// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;
using Microsoft.CodeAnalysis;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_FactMustNotModifySharedContextAnalyzer.when_analyzing;

public class and_fact_changes_shared_state_through_a_null_conditional_access : given.a_fact_must_not_modify_shared_context_analyzer
{
    const string Usage = """
    public class Owner : System.IDisposable
    {
        public int Count { get; set; }
        public Owner Inner { get; set; }
        public void Dispose() { }
    }

    public class when_something : Cratis.Specifications.Specification
    {
        System.Collections.Generic.List<int> _items;
        System.Collections.Generic.Dictionary<string, int> _counts;
        Owner _owner;

        [Xunit.Fact]
        void should_change()
        {
            {|#0:_items?.Clear()|};
            {|#1:_owner?.Count = 1|};
            {|#2:_counts?["a"] = 1|};
            {|#3:_owner?.Inner?.Dispose()|};
        }
    }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<FactMustNotModifySharedContextAnalyzer>.VerifyAnalyzerOnCompilingSource(
        SpecSource.Wrap(Usage),
        new ExpectedDiagnostic(DiagnosticIds.FactMustNotModifySharedContext, DiagnosticSeverity.Error, "should_change", "'_items'"),
        new ExpectedDiagnostic(DiagnosticIds.FactMustNotModifySharedContext, DiagnosticSeverity.Error, "should_change", "'_owner'"),
        new ExpectedDiagnostic(DiagnosticIds.FactMustNotModifySharedContext, DiagnosticSeverity.Error, "should_change", "'_counts'"),
        new ExpectedDiagnostic(DiagnosticIds.FactMustNotModifySharedContext, DiagnosticSeverity.Error, "should_change", "'_owner'"));

    [Fact] Task should_report_each_change() => _result;
}
