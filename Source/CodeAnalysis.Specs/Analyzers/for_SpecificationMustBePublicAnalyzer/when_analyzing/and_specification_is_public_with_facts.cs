// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_SpecificationMustBePublicAnalyzer.when_analyzing;

public class and_specification_is_public_with_facts : given.a_specification_must_be_public_analyzer
{
    const string Usage = """
    public class when_doing_something : Cratis.Specifications.Specification
    {
        [Xunit.Fact] void should_do_something() { }
    }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<SpecificationMustBePublicAnalyzer>.VerifyAnalyzer(SpecSource.Wrap(Usage));

    [Fact] Task should_not_report_any_diagnostics() => _result;
}
