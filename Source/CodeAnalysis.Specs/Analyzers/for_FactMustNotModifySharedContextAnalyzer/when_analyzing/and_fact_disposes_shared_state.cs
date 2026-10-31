// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;
using Microsoft.CodeAnalysis;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_FactMustNotModifySharedContextAnalyzer.when_analyzing;

public class and_fact_disposes_shared_state : given.a_fact_must_not_modify_shared_context_analyzer
{
    const string Usage = """
    public class Resource : System.IDisposable, System.IAsyncDisposable
    {
        public Resource Inner { get; set; }
        public void Dispose() { }
        public System.Threading.Tasks.ValueTask DisposeAsync() => default;
    }

    public class when_something : Cratis.Specifications.Specification
    {
        Resource _resource;

        [Xunit.Fact]
        async System.Threading.Tasks.Task should_release()
        {
            {|#0:_resource.Dispose()|};
            await {|#1:_resource.Inner.DisposeAsync()|};
        }
    }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<FactMustNotModifySharedContextAnalyzer>.VerifyAnalyzerOnCompilingSource(
        SpecSource.Wrap(Usage),
        new ExpectedDiagnostic(DiagnosticIds.FactMustNotModifySharedContext, DiagnosticSeverity.Error, "should_release", "'_resource'"),
        new ExpectedDiagnostic(DiagnosticIds.FactMustNotModifySharedContext, DiagnosticSeverity.Error, "should_release", "'_resource'"));

    [Fact] Task should_report_each_disposal() => _result;
}
