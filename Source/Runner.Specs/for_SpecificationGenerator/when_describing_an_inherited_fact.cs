// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_SpecificationGenerator;

public class when_describing_an_inherited_fact : Specification
{
    SpecificationClassDescriptor descriptor;
    IReadOnlyList<string> uids;

    void Because()
    {
        descriptor = given.the_probes.Get<inheriting_a_fact>();
        uids = [.. given.the_probes.All.Select(_ => _.Uid)];
    }

    void should_list_the_facts_of_the_base_before_those_of_the_derived_type() =>
        descriptor.Facts.Select(_ => _.MethodName).ShouldEqual("should_be_declared_on_the_base", "should_be_declared_on_the_derived_type");

    void should_register_the_runnable_class() => descriptor.Uid.ShouldEqual(typeof(inheriting_a_fact).FullName);

    void should_not_register_the_abstract_base() => uids.ShouldNotContain(typeof(a_context_with_a_fact).FullName);
}
