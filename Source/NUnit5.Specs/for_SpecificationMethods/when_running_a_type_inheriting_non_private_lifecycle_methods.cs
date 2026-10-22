// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.for_SpecificationMethods.probes;

namespace Cratis.Specifications.for_SpecificationMethods;

public class when_running_a_type_inheriting_non_private_lifecycle_methods : Specification
{
    probe_derived_from_protected_lifecycle_without_its_own _probe;

    void Establish() => _probe = new();

    Task Because() => LifecycleRun.All(_probe);

    [Test] public void should_run_each_inherited_method_only_once() => _probe.Calls.ShouldEqual("base.Establish", "base.Because", "base.Destroy");
}
