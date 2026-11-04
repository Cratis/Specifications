// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;
using Microsoft.CodeAnalysis;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_DoNotCallBaseLifecycleMethodAnalyzer;

public class when_calling_base_when : Specification
{
    Task _result;
    void Because() => _result = AnalyzerVerifier<DoNotCallBaseLifecycleMethodAnalyzer>.VerifyAnalyzer(
        SpecSource.Wrap("""
        public class a_context : Cratis.Specifications.Specification { protected void When() { } } public class when_acting : a_context { void Because() => base.{|#0:When|}(); }
        """),
        new ExpectedDiagnostic(DiagnosticIds.DoNotCallBaseLifecycleMethod, DiagnosticSeverity.Warning, "When"));
    [Fact] Task should_apply_the_lifecycle_rule() => _result;
}
