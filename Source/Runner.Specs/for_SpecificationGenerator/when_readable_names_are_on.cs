// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_SpecificationGenerator;

public class when_readable_names_are_on : Specification
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

    void Because() => generated = given.a_generator_run.Over(Source, readableNames: true).Source;

    void should_use_the_readable_class_name() => generated.ShouldContain("\"Author \u203A when registering \u203A and the name is taken\"");

    void should_use_the_readable_fact_name() => generated.ShouldContain("\"Author \u203A when registering \u203A and the name is taken \u203A should fail\"");

    void should_keep_the_uid() => generated.ShouldContain("\"Probes.for_Author.when_registering.and_the_name_is_taken\"");
}
