// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_SpecificationClassRunner;

public class when_running_a_synchronous_specification : given.a_probe_run<synchronous_specification>
{
    void should_pass_every_fact() => results.ShouldEachConformTo(_ => _.Outcome == FactOutcome.Passed);

    void should_find_facts_by_convention_and_by_attribute() => work.Facts.Select(_ => _.MethodName).ShouldContainOnly("should_be_the_first_fact", "should_be_the_second_fact", "third_fact_with_attribute");

    void should_establish_and_give_once_from_the_base_type_and_then_act_once() =>
        calls.Take(4).ShouldEqual("Establish base", "Given", "Because base", "When");

    void should_run_every_fact_against_the_shared_context() => calls.Skip(4).Take(3).ShouldEqual("fact", "fact", "fact");

    void should_destroy_once_from_the_base_type_and_then_dispose() => calls.Skip(7).ShouldEqual("Destroy base", "Destroy", "Dispose");
}
