// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.for_SpecificationMethods.probes;

namespace Cratis.Specifications.for_SpecificationMethods;

public class when_running_a_type_with_overloaded_lifecycle_methods : Specification
{
    probe_with_overloaded_lifecycle _probe;

    void Establish() => _probe = new();

    Task Because() => LifecycleRun.All(_probe);

    [Test] public void should_only_run_the_parameterless_method() => _probe.Calls.ShouldEqual("Establish");
}
