// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_SpecificationGenerator;

public class when_a_fact_is_async_void : Specification
{
    const string Source = """
        public class an_async_void_specification : Cratis.Specifications.Specification
        {
            async void Establish() => await System.Threading.Tasks.Task.Yield();

            async void should_not_be_async_void() => await System.Threading.Tasks.Task.Yield();

            void should_be_fine() { }
        }
        """;

    IReadOnlyList<Microsoft.CodeAnalysis.Diagnostic> diagnostics;

    void Because() => diagnostics = [.. given.a_generator_run.Over(Source).Diagnostics.Where(_ => _.Id == "CRSPEC0010")];

    void should_report_the_fact_and_the_lifecycle_method() => diagnostics.Count.ShouldEqual(2);

    void should_report_an_error() => diagnostics.ShouldEachConformTo(_ => _.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);

    void should_name_the_method() => diagnostics.Select(_ => _.GetMessage()).ShouldEachConformTo(_ => _.Contains("an_async_void_specification."));
}
