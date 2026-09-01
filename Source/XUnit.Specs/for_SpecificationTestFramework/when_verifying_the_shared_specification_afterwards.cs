// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_SpecificationTestFramework;

// Runs after when_running_facts_on_a_shared_specification - collections in this assembly run
// sequentially in alphabetical order (see TestFrameworkConfiguration.cs).
public class when_verifying_the_shared_specification_afterwards : Specification
{
    [Fact] void should_have_run_destroy_once_after_the_last_fact() => when_running_facts_on_a_shared_specification.destroy_count.ShouldEqual(1);

    [Fact] void should_not_have_run_establish_again() => when_running_facts_on_a_shared_specification.establish_count.ShouldEqual(1);

    [Fact] void should_not_have_run_because_again() => when_running_facts_on_a_shared_specification.because_count.ShouldEqual(1);

    [Fact] void should_not_have_constructed_the_class_again() => when_running_facts_on_a_shared_specification.instances_created.ShouldEqual(1);
}
