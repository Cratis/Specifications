// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;
using Microsoft.CodeAnalysis;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_BecauseMustNotBeOnGivenContextAnalyzer.when_analyzing;

public class and_because_is_on_a_given_context : given.a_because_must_not_be_on_given_context_analyzer
{
    const string Usage = """
    public class a_reusable_context : Cratis.Specifications.Specification
    {
        void {|#0:Because|}() { }
    }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<BecauseMustNotBeOnGivenContextAnalyzer>.VerifyAnalyzer(
        SpecSource.Wrap(Usage, "Sample.for_something.given"),
        new ExpectedDiagnostic(DiagnosticIds.BecauseMustNotBeOnGivenContext, DiagnosticSeverity.Warning, "a_reusable_context"));

    [Fact] Task should_report_the_because_on_given_context() => _result;
}
