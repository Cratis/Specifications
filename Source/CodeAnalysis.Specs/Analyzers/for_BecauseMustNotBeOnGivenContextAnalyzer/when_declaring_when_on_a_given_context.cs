// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;
using Microsoft.CodeAnalysis;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_BecauseMustNotBeOnGivenContextAnalyzer;

public class when_declaring_when_on_a_given_context : Specification
{
    Task _result;
    void Because() => _result = AnalyzerVerifier<BecauseMustNotBeOnGivenContextAnalyzer>.VerifyAnalyzer(
        SpecSource.Wrap("""
        namespace Example.given { public class a_context : Cratis.Specifications.Specification { void {|#0:When|}() { } } }
        """),
        new ExpectedDiagnostic(DiagnosticIds.BecauseMustNotBeOnGivenContext, DiagnosticSeverity.Warning, "a_context"));
    [Fact] Task should_apply_the_lifecycle_rule() => _result;
}
