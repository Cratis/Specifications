// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_SpecificationGenerator;

public class when_readable_names_are_off : Specification
{
    const string Source = """
        namespace Probes.for_Author.when_registering
        {
            public class and_the_name_is_taken : Cratis.Specifications.Specification
            {
                void should_fail() { }
            }
        }
        """;

    string generated;

    void Because() => generated = given.a_generator_run.Over(Source, readableNames: false).Source;

    void should_use_the_raw_class_name() => generated.ShouldContain("\"Probes.for_Author.when_registering.and_the_name_is_taken\"");

    void should_use_the_raw_fact_name() => generated.ShouldContain("\"Probes.for_Author.when_registering.and_the_name_is_taken \u203A should_fail\"");

    void should_not_use_the_readable_names() => generated.ShouldNotContain("Author \u203A");
}
