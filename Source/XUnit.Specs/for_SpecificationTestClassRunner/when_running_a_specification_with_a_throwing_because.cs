// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.for_SpecificationTestClassRunner.probes;
using Xunit.Abstractions;

namespace Cratis.Specifications.for_SpecificationTestClassRunner;

public class when_running_a_specification_with_a_throwing_because : given.a_class_runner_harness
{
    void Establish() => shared_specification_with_a_throwing_because.Reset();

    Task Because() => RunClass(typeof(shared_specification_with_a_throwing_because));

    [Fact] void should_fail_all_facts() => message_bus.Messages.OfType<ITestFailed>().Count().ShouldEqual(2);

    [Fact] void should_not_pass_any_fact() => message_bus.Messages.OfType<ITestPassed>().ShouldBeEmpty();

    [Fact] void should_fail_every_fact_with_the_because_exception() =>
        message_bus.Messages.OfType<ITestFailed>().ShouldEachConformTo(_ => _.ExceptionTypes.Contains(typeof(InvalidOperationException).FullName));

    [Fact] void should_not_execute_any_fact_body() => shared_specification_with_a_throwing_because.facts_executed.ShouldEqual(0);

    [Fact] void should_only_attempt_because_once() => shared_specification_with_a_throwing_because.because_count.ShouldEqual(1);

    [Fact] void should_still_run_destroy_once() => shared_specification_with_a_throwing_because.destroy_count.ShouldEqual(1);
}
