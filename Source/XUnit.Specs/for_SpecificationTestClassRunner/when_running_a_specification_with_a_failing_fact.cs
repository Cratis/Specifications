// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.for_SpecificationTestClassRunner.probes;
using Xunit.Abstractions;

namespace Cratis.Specifications.for_SpecificationTestClassRunner;

public class when_running_a_specification_with_a_failing_fact : given.a_class_runner_harness
{
    void Establish() => shared_specification_with_a_failing_fact.Reset();

    Task Because() => RunClass(typeof(shared_specification_with_a_failing_fact));

    [Fact] void should_fail_only_the_failing_fact() => message_bus.Messages.OfType<ITestFailed>().Count().ShouldEqual(1);

    [Fact] void should_fail_the_fact_that_threw() => message_bus.Messages.OfType<ITestFailed>().Single().Test.DisplayName.ShouldContain(nameof(shared_specification_with_a_failing_fact.failing_fact));

    [Fact] void should_fail_with_the_thrown_exception() => message_bus.Messages.OfType<ITestFailed>().Single().ExceptionTypes.ShouldContain(typeof(InvalidOperationException).FullName);

    [Fact] void should_pass_the_other_facts() => message_bus.Messages.OfType<ITestPassed>().Count().ShouldEqual(2);

    [Fact] void should_run_establish_once() => shared_specification_with_a_failing_fact.establish_count.ShouldEqual(1);

    [Fact] void should_run_destroy_once() => shared_specification_with_a_failing_fact.destroy_count.ShouldEqual(1);
}
