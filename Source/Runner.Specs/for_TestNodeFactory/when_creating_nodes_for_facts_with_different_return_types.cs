// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Testing.Platform.Extensions.Messages;

namespace Cratis.Specifications.Runner.for_TestNodeFactory;

public class when_creating_nodes_for_facts_with_different_return_types : Specification
{
    SpecificationClassDescriptor descriptor;
    TestNode asynchronous;
    TestNode value_task;
    TestNode synchronous;
    TestNode inherited;

    void Because()
    {
        var factory = new TestNodeFactory("Probes", new TrxCapability());
        var asynchronous_descriptor = given.the_probes.Get<asynchronous_specification>();
        descriptor = given.the_probes.Get<inheriting_a_fact>();
        asynchronous = factory.Create(asynchronous_descriptor, Fact(asynchronous_descriptor, "should_run_asynchronously"), PassedTestNodeStateProperty.CachedInstance);
        value_task = factory.Create(asynchronous_descriptor, Fact(asynchronous_descriptor, "should_run_as_a_value_task"), PassedTestNodeStateProperty.CachedInstance);
        synchronous = factory.Create(descriptor, Fact(descriptor, "should_be_declared_on_the_derived_type"), PassedTestNodeStateProperty.CachedInstance);
        inherited = factory.Create(descriptor, Fact(descriptor, "should_be_declared_on_the_base"), PassedTestNodeStateProperty.CachedInstance);
    }

    static FactDescriptor Fact(SpecificationClassDescriptor specification, string method) => specification.Facts.Single(_ => _.MethodName == method);

    static TestMethodIdentifierProperty Identity(TestNode node) => node.Properties.Single<TestMethodIdentifierProperty>();

    void should_identify_a_task_fact_with_its_return_type() => Identity(asynchronous).ReturnTypeFullName.ShouldEqual("System.Threading.Tasks.Task");

    void should_identify_a_value_task_fact_with_its_return_type() => Identity(value_task).ReturnTypeFullName.ShouldEqual("System.Threading.Tasks.ValueTask");

    void should_identify_a_synchronous_fact_with_void() => Identity(synchronous).ReturnTypeFullName.ShouldEqual("System.Void");

    void should_identify_an_inherited_fact_by_the_runnable_class() => Identity(inherited).TypeName.ShouldEqual(nameof(inheriting_a_fact));

    void should_keep_the_method_name_of_an_inherited_fact() => Identity(inherited).MethodName.ShouldEqual("should_be_declared_on_the_base");
}
