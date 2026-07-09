// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_SpecificationMustBePublicAnalyzer.when_analyzing;

public class and_internal_specification_has_no_facts : given.a_specification_must_be_public_analyzer
{
    const string Usage = """
    internal class a_reusable_context : Cratis.Specifications.Specification
    {
        protected void Establish() { }
    }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<SpecificationMustBePublicAnalyzer>.VerifyAnalyzer(SpecSource.Wrap(Usage));

    [Fact] Task should_not_report_any_diagnostics() => _result;
}
