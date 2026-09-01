// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.for_SpecificationTestClassRunner.probes;
using Xunit.Abstractions;

namespace Cratis.Specifications.for_SpecificationTestClassRunner;

public class when_running_a_disposable_specification : given.a_class_runner_harness
{
    void Establish() => disposable_specification_with_two_facts.Reset();

    Task Because() => RunClass(typeof(disposable_specification_with_two_facts));

    [Fact] void should_pass_all_facts() => message_bus.Messages.OfType<ITestPassed>().Count().ShouldEqual(2);

    [Fact] void should_fall_back_to_a_fresh_instance_per_fact() => disposable_specification_with_two_facts.instances_created.ShouldEqual(2);

    [Fact] void should_run_establish_per_fact() => disposable_specification_with_two_facts.establish_count.ShouldEqual(2);

    [Fact] void should_dispose_each_instance() => disposable_specification_with_two_facts.dispose_count.ShouldEqual(2);
}
