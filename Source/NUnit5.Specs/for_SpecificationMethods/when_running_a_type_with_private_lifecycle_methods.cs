// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.for_SpecificationMethods.probes;

namespace Cratis.Specifications.for_SpecificationMethods;

public class when_running_a_type_with_private_lifecycle_methods : Specification
{
    probe_with_private_lifecycle _probe;

    void Establish() => _probe = new();

    Task Because() => LifecycleRun.All(_probe);

    [Test] public void should_run_establish_because_and_destroy_in_order() => _probe.Calls.ShouldEqual("Establish", "Because", "Destroy");
}
