// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.for_SpecificationTestClassRunner.probes;
using Xunit.Sdk;

namespace Cratis.Specifications.for_SpecificationTestClassRunner;

public class when_running_a_specification_skipped_when_an_inherited_condition_holds : given.a_class_runner_harness
{
    void Establish() => skipping_probe_counts.Reset();

    Task Because() => RunClass(typeof(shared_specification_skipped_when_an_inherited_condition_holds));

    [Fact] void should_skip_every_fact() => message_bus.Messages.OfType<ITestSkipped>().Count().ShouldEqual(2);

    [Fact] void should_not_construct_the_class() => skipping_probe_counts.instances_created.ShouldEqual(0);

    [Fact] void should_not_run_establish() => skipping_probe_counts.establish_count.ShouldEqual(0);
}
