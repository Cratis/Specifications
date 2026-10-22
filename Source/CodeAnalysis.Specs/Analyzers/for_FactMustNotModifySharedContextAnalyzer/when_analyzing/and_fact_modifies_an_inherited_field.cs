// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;
using Microsoft.CodeAnalysis;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_FactMustNotModifySharedContextAnalyzer.when_analyzing;

public class and_fact_modifies_an_inherited_field : given.a_fact_must_not_modify_shared_context_analyzer
{
    const string Usage = """
    public class a_context : Cratis.Specifications.Specification
    {
        protected int _value;
    }

    public class when_something : a_context
    {
        [NUnit.Framework.Test] void should_change() => {|#0:_value = 1|};
    }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<FactMustNotModifySharedContextAnalyzer>.VerifyAnalyzer(
        SpecSource.Wrap(Usage, "Sample.when_something"),
        new ExpectedDiagnostic(DiagnosticIds.FactMustNotModifySharedContext, DiagnosticSeverity.Info, "should_change", "_value"));

    [Fact] Task should_report_the_inherited_field_assignment() => _result;
}
