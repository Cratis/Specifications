// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.for_SpecificationMethods.probes;

namespace Cratis.Specifications.for_SpecificationMethods;

public class when_running_a_type_inheriting_private_lifecycle_methods : Specification
{
    probe_derived_with_private_lifecycle _probe;

    void Establish() => _probe = new();

    Task Because() => LifecycleRun.All(_probe);

    [Test] public void should_run_each_method_once_starting_at_the_base_type() => _probe.Calls.ShouldEqual("base.Establish", "derived.Establish", "base.Because", "derived.Because", "base.Destroy", "derived.Destroy");
}
