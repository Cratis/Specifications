// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.for_SpecificationMethods.probes;

namespace Cratis.Specifications.for_SpecificationMethods;

public class when_running_a_type_overriding_a_virtual_lifecycle_method : Specification
{
    probe_overriding_virtual_lifecycle _probe;

    void Establish() => _probe = new();

    Task Because() => LifecycleRun.All(_probe);

    [Test] public void should_run_the_override_only_once() => _probe.Calls.ShouldEqual("derived.Establish");
}
