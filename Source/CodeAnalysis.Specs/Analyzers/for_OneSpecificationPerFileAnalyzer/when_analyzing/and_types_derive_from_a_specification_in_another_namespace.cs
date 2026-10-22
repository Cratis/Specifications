// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_OneSpecificationPerFileAnalyzer.when_analyzing;

public class and_types_derive_from_a_specification_in_another_namespace : given.an_one_specification_per_file_analyzer
{
    const string Usage = """
    public class Specification { }
    public class when_doing_the_first_thing : Specification { }
    public class when_doing_the_second_thing : Specification { }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<OneSpecificationPerFileAnalyzer>.VerifyAnalyzer(SpecSource.Wrap(Usage));

    [Fact] Task should_not_report_any_diagnostics() => _result;
}
