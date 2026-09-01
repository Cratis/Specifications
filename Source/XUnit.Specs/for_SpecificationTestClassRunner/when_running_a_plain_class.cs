// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.for_SpecificationTestClassRunner.probes;
using Xunit.Abstractions;

namespace Cratis.Specifications.for_SpecificationTestClassRunner;

public class when_running_a_plain_class : given.a_class_runner_harness
{
    void Establish() => plain_class_with_two_facts.Reset();

    Task Because() => RunClass(typeof(plain_class_with_two_facts));

    [Fact] void should_pass_all_facts() => message_bus.Messages.OfType<ITestPassed>().Count().ShouldEqual(2);

    [Fact] void should_construct_a_fresh_instance_per_fact() => plain_class_with_two_facts.instances_created.ShouldEqual(2);
}
