// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_SpecificationTestFramework;

public class when_running_facts_on_a_shared_specification : Specification
{
    internal static int establish_count;
    internal static int because_count;
    internal static int destroy_count;
    internal static int instances_created;

    public when_running_facts_on_a_shared_specification() => instances_created++;

    void Establish() => establish_count++;

    void Because() => because_count++;

    void Destroy() => destroy_count++;

    [Fact] void should_construct_the_class_once() => instances_created.ShouldEqual(1);

    [Fact] void should_run_establish_once() => establish_count.ShouldEqual(1);

    [Fact] void should_run_because_once() => because_count.ShouldEqual(1);

    [Fact] void should_not_run_destroy_while_facts_are_running() => destroy_count.ShouldEqual(0);
}
