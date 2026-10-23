// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_SpecificationGenerator;

public class when_describing_the_return_types_of_facts : Specification
{
    IReadOnlyList<FactDescriptor> facts;

    void Because() => facts = given.the_probes.Get<asynchronous_specification>().Facts;

    string ReturnTypeOf(string method) => facts.Single(_ => _.MethodName == method).ReturnTypeFullName;

    void should_carry_task_for_a_task_fact() => ReturnTypeOf("should_run_asynchronously").ShouldEqual("System.Threading.Tasks.Task");

    void should_carry_value_task_for_a_value_task_fact() => ReturnTypeOf("should_run_as_a_value_task").ShouldEqual("System.Threading.Tasks.ValueTask");

    void should_carry_void_for_a_synchronous_fact() =>
        given.the_probes.Get<synchronous_specification>().Facts[0].ReturnTypeFullName.ShouldEqual("System.Void");
}
