// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.for_SpecificationTestClassRunner.probes;
using Xunit.Abstractions;

namespace Cratis.Specifications.for_SpecificationTestClassRunner;

public class when_running_a_specification_with_one_skipped_fact : given.a_class_runner_harness
{
    void Establish() => shared_specification_with_a_skipped_fact.Reset();

    Task Because() => RunClass(typeof(shared_specification_with_a_skipped_fact));

    [Fact] void should_pass_the_facts_that_run() => message_bus.Messages.OfType<ITestPassed>().Count().ShouldEqual(2);

    [Fact] void should_report_the_skipped_fact() => message_bus.Messages.OfType<ITestSkipped>().Count().ShouldEqual(1);

    [Fact] void should_construct_the_class_once() => shared_specification_with_a_skipped_fact.instances_created.ShouldEqual(1);

    [Fact] void should_run_establish_once() => shared_specification_with_a_skipped_fact.establish_count.ShouldEqual(1);

    [Fact] void should_run_because_once() => shared_specification_with_a_skipped_fact.because_count.ShouldEqual(1);
}
