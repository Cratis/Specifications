// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.for_SpecificationMethods.probes;

namespace Cratis.Specifications.for_SpecificationMethods;

public class when_running_a_type_with_async_lifecycle_methods : Specification
{
    probe_with_async_lifecycle _probe;

    void Establish() => _probe = new();

    Task Because() => LifecycleRun.All(_probe);

    [Test] public void should_await_task_and_value_task_methods() => _probe.Calls.ShouldEqual("Establish.started", "Establish.completed", "Because.started", "Because.completed", "Destroy.started", "Destroy.completed");
}
