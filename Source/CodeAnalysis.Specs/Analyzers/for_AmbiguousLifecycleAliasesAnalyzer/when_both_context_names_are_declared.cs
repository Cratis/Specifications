// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;
using Microsoft.CodeAnalysis;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_AmbiguousLifecycleAliasesAnalyzer;

public class when_both_context_names_are_declared : Specification
{
    Task _result;
    void Because() => _result = AnalyzerVerifier<AmbiguousLifecycleAliasesAnalyzer>.VerifyAnalyzer(
        SpecSource.Wrap("""
        public class when_acting : Cratis.Specifications.Specification { void Establish() { } void {|#0:Given|}() { } }
        """),
        new ExpectedDiagnostic(DiagnosticIds.AmbiguousLifecycleAliases, DiagnosticSeverity.Error, "when_acting", "Establish", "Given"));
    [Fact] Task should_apply_the_lifecycle_rule() => _result;
}
