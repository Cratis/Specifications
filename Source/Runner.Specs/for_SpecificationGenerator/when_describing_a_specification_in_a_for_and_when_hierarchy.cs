// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.Probes.for_Author.when_registering;

namespace Cratis.Specifications.Runner.for_SpecificationGenerator;

public class when_describing_a_specification_in_a_for_and_when_hierarchy : Specification
{
    SpecificationClassDescriptor specification;
    SpecificationClassDescriptor nested;

    void Because()
    {
        specification = given.the_probes.Get<and_the_name_is_taken>();
        nested = given.the_probes.Get<and_the_name_is_taken.and_the_author_is_retired>();
    }

    void should_use_the_fully_qualified_name_as_the_class_uid() => specification.Uid.ShouldEqual("Cratis.Specifications.Probes.for_Author.when_registering.and_the_name_is_taken");

    void should_use_the_class_uid_and_method_name_as_the_fact_uid() => specification.Facts[0].Uid.ShouldEqual("Cratis.Specifications.Probes.for_Author.when_registering.and_the_name_is_taken.should_fail");

    void should_read_as_a_sentence_from_the_subject() => specification.Facts[0].DisplayName.ShouldEqual("Author \u203A when registering \u203A and the name is taken \u203A should fail");

    void should_separate_nested_types_with_a_plus_in_the_uid() => nested.Uid.ShouldEqual("Cratis.Specifications.Probes.for_Author.when_registering.and_the_name_is_taken+and_the_author_is_retired");

    void should_read_nested_types_as_further_conditions() => nested.Facts[0].DisplayName.ShouldEqual("Author \u203A when registering \u203A and the name is taken \u203A and the author is retired \u203A should still fail");

    void should_point_to_the_source_file_of_the_fact() => Path.GetFileName(specification.Facts[0].FilePath).ShouldEqual("and_the_name_is_taken.cs");

    void should_point_to_the_line_of_the_fact() => specification.Facts[0].Line.ShouldEqual(8);
}
