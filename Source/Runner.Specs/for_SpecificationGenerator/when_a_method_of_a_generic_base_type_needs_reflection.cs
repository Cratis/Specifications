// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_SpecificationGenerator;

public class when_a_method_of_a_generic_base_type_needs_reflection : Specification
{
    const string Source = """
        public abstract class a_generic_context<T> : Cratis.Specifications.Specification
        {
            void Establish() { }

            protected void should_be_inherited() { }
        }

        public class a_reflecting_specification : a_generic_context<int>
        {
            void should_be_fine() { }
        }
        """;

    IReadOnlyList<Microsoft.CodeAnalysis.Diagnostic> diagnostics;

    void Because() => diagnostics = [.. given.a_generator_run.Over(Source).Diagnostics.Where(_ => _.Id == "CRSPEC0011")];

    void should_report_each_method_called_through_reflection() => diagnostics.Count.ShouldEqual(2);

    void should_report_a_warning() => diagnostics.ShouldEachConformTo(_ => _.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Warning);
}
