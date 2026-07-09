// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_BecauseMustNotBeOnGivenContextAnalyzer.when_analyzing;

public class and_because_is_on_a_when_specification : given.a_because_must_not_be_on_given_context_analyzer
{
    const string Usage = """
    public class when_doing_something : Cratis.Specifications.Specification
    {
        void Because() { }
    }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<BecauseMustNotBeOnGivenContextAnalyzer>.VerifyAnalyzer(
        SpecSource.Wrap(Usage, "Sample.for_something"));

    [Fact] Task should_not_report_any_diagnostics() => _result;
}
