// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.for_SpecificationTestClassRunner.probes;
using Xunit.Abstractions;

namespace Cratis.Specifications.for_SpecificationTestClassRunner;

public class when_running_a_specification_with_multiple_facts : given.a_class_runner_harness
{
    void Establish() => shared_specification_with_three_facts.Reset();

    Task Because() => RunClass(typeof(shared_specification_with_three_facts));

    [Fact] void should_pass_all_facts() => message_bus.Messages.OfType<ITestPassed>().Count().ShouldEqual(3);

    [Fact] void should_not_fail_any_fact() => message_bus.Messages.OfType<ITestFailed>().ShouldBeEmpty();

    [Fact] void should_construct_the_class_once() => shared_specification_with_three_facts.instances_created.ShouldEqual(1);

    [Fact] void should_run_establish_once() => shared_specification_with_three_facts.establish_count.ShouldEqual(1);

    [Fact] void should_run_because_once() => shared_specification_with_three_facts.because_count.ShouldEqual(1);

    [Fact] void should_run_destroy_once() => shared_specification_with_three_facts.destroy_count.ShouldEqual(1);

    [Fact] void should_run_the_lifecycle_in_order_with_destroy_after_the_last_fact() =>
        string.Join(",", shared_specification_with_three_facts.lifecycle_order).ShouldEqual("establish,because,fact,fact,fact,destroy");
}
