// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.for_SpecificationTestClassRunner.probes;
using Xunit.Sdk;

namespace Cratis.Specifications.for_SpecificationTestClassRunner;

public class when_running_a_per_fact_specification : given.a_class_runner_harness
{
    void Establish() => per_fact_specification_with_two_facts.Reset();

    Task Because() => RunClass(typeof(per_fact_specification_with_two_facts));

    [Fact] void should_pass_all_facts() => message_bus.Messages.OfType<ITestPassed>().Count().ShouldEqual(2);

    [Fact] void should_construct_a_fresh_instance_per_fact() => per_fact_specification_with_two_facts.instances_created.ShouldEqual(2);

    [Fact] void should_establish_per_fact() => per_fact_specification_with_two_facts.establish_count.ShouldEqual(2);

    [Fact] void should_run_because_per_fact() => per_fact_specification_with_two_facts.because_count.ShouldEqual(2);

    [Fact] void should_destroy_per_fact() => per_fact_specification_with_two_facts.destroy_count.ShouldEqual(2);
}
