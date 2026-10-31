// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;
using Microsoft.CodeAnalysis;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_FactMustNotModifySharedContextAnalyzer.when_analyzing;

public class and_fact_assigns_through_a_nested_member : given.a_fact_must_not_modify_shared_context_analyzer
{
    const string Usage = """
    public class Owner
    {
        public Owner Child { get; set; }
        public int Count { get; set; }
        public event System.EventHandler Changed;
    }

    public class when_something : Cratis.Specifications.Specification
    {
        Owner _owner;

        Owner Shared { get; set; }

        [Xunit.Fact]
        void should_change()
        {
            {|#0:_owner.Count = 1|};
            {|#1:this._owner.Child.Count += 2|};
            {|#2:_owner.Count++|};
            {|#3:Shared.Child = null|};
            {|#4:_owner.Changed += (_, _) => { }|};
        }
    }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<FactMustNotModifySharedContextAnalyzer>.VerifyAnalyzerOnCompilingSource(
        SpecSource.Wrap(Usage),
        new ExpectedDiagnostic(DiagnosticIds.FactMustNotModifySharedContext, DiagnosticSeverity.Error, "should_change", "'_owner'"),
        new ExpectedDiagnostic(DiagnosticIds.FactMustNotModifySharedContext, DiagnosticSeverity.Error, "should_change", "'_owner'"),
        new ExpectedDiagnostic(DiagnosticIds.FactMustNotModifySharedContext, DiagnosticSeverity.Error, "should_change", "'_owner'"),
        new ExpectedDiagnostic(DiagnosticIds.FactMustNotModifySharedContext, DiagnosticSeverity.Error, "should_change", "'Shared'"),
        new ExpectedDiagnostic(DiagnosticIds.FactMustNotModifySharedContext, DiagnosticSeverity.Error, "should_change", "'_owner'"));

    [Fact] Task should_report_each_change_to_an_object_reached_through_a_shared_member() => _result;
}
