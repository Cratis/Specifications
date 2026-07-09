// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_OneSpecificationPerFileAnalyzer.when_analyzing;

public class and_second_type_is_a_spec_helpers : given.an_one_specification_per_file_analyzer
{
    const string Usage = """
    public class when_doing_something : Cratis.Specifications.Specification { }
    public class SomethingSpecHelpers : Cratis.Specifications.Specification { }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<OneSpecificationPerFileAnalyzer>.VerifyAnalyzer(SpecSource.Wrap(Usage));

    [Fact] Task should_not_report_any_diagnostics() => _result;
}
