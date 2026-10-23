// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.for_SpecificationMethods.probes;

namespace Cratis.Specifications.for_SpecificationMethods;

public class when_running_lifecycle_aliases : Specification
{
    readonly probe_with_aliases _probe = new();

    Task Because() => LifecycleRun.All(_probe);

    [Fact] void should_run_given_when_and_destroy() => _probe.Calls.ShouldEqual("Given", "When", "Destroy");
}
