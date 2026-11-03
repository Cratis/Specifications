// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.for_SpecificationTestClassRunner.probes;
using Xunit.Abstractions;

namespace Cratis.Specifications.for_SpecificationTestClassRunner;

public class when_running_a_specification_that_skips_every_fact : given.a_class_runner_harness
{
    void Establish() => shared_specification_with_every_fact_skipped.Reset();

    Task Because() => RunClass(typeof(shared_specification_with_every_fact_skipped));

    [Fact] void should_report_every_fact_as_skipped() => message_bus.Messages.OfType<ITestSkipped>().Count().ShouldEqual(2);

    [Fact] void should_not_construct_the_class() => shared_specification_with_every_fact_skipped.instances_created.ShouldEqual(0);

    [Fact] void should_not_run_establish() => shared_specification_with_every_fact_skipped.establish_count.ShouldEqual(0);

    [Fact] void should_not_run_because() => shared_specification_with_every_fact_skipped.because_count.ShouldEqual(0);
}
