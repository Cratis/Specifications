// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_SpecificationMethods;

public class when_formatting_specification_display_names : Specification
{
    [Fact] void should_trim_the_namespace_before_the_subject() => SpecificationDisplayNames.ForClass("Company.for_Author.when_registering", ["and_the_name_is_taken"]).ShouldEqual("Author › when registering › and the name is taken");
    [Fact] void should_support_types_without_a_subject_namespace() => SpecificationDisplayNames.ForClass("Company.Tests", ["when_registering"]).ShouldEqual("when registering");
    [Fact] void should_collapse_repeated_underscores() => SpecificationDisplayNames.Readable("__should__work__").ShouldEqual("should work");
    [Fact] void should_not_add_another_space_after_an_existing_space() => SpecificationDisplayNames.Readable("should _work").ShouldEqual("should work");
    [Fact] void should_support_an_identifier_of_only_underscores() => SpecificationDisplayNames.Readable("___").ShouldEqual(string.Empty);
    [Fact] void should_format_fact_names() => SpecificationDisplayNames.ForFact("Author", "should_fail").ShouldEqual("Author › should fail");
    [Fact] void should_include_nested_runtime_types() => SpecificationDisplayNames.ForType(typeof(when_formatting_specification_display_names)).ShouldEqual("SpecificationMethods › when formatting specification display names");
}
