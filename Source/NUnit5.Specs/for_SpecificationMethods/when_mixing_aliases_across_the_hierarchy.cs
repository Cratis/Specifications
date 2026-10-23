// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.for_SpecificationMethods.probes;

namespace Cratis.Specifications.for_SpecificationMethods;

public class when_mixing_aliases_across_the_hierarchy : Specification
{
    readonly probe_with_mixed_aliases _probe = new();

    Task Because() => LifecycleRun.All(_probe);

    [Test] public void should_run_base_first() => _probe.Calls.ShouldEqual("Given", "derived.Establish", "When", "Destroy");
}
