// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.CodeAnalysis.Analyzers;
using Cratis.Specifications.CodeAnalysis.Specs.Testing;
using Microsoft.CodeAnalysis;

namespace Cratis.Specifications.CodeAnalysis.Specs.Analyzers.for_OneSpecificationPerFileAnalyzer.when_analyzing;

public class and_file_has_two_specifications : given.an_one_specification_per_file_analyzer
{
    const string Usage = """
    public class when_doing_the_first_thing : Cratis.Specifications.Specification { }
    public class {|#0:when_doing_the_second_thing|} : Cratis.Specifications.Specification { }
    """;

    Task _result;

    void Because() => _result = AnalyzerVerifier<OneSpecificationPerFileAnalyzer>.VerifyAnalyzer(
        SpecSource.Wrap(Usage),
        new ExpectedDiagnostic(DiagnosticIds.OneSpecificationPerFile, DiagnosticSeverity.Warning, "when_doing_the_second_thing"));

    [Fact] Task should_report_the_second_specification() => _result;
}
