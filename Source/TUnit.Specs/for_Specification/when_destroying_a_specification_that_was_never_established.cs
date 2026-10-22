// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.for_Specification.probes;

namespace Cratis.Specifications.for_Specification;

public class when_destroying_a_specification_that_was_never_established : given.a_specification_class_constructor
{
    // A probe per spec, as the shared instance is cached per class.
    sealed class Probe : recording_specification;

    Probe _specification;

    async Task Establish() => _specification = await Create<Probe>();

    Task Because() => Specification.DestroyAsync(typeof(Probe));

    [Test] public void should_not_run_any_lifecycle_methods() => _specification.Calls.ShouldBeEmpty();
}
