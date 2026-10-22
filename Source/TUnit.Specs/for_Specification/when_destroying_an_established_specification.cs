// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.for_Specification.probes;

namespace Cratis.Specifications.for_Specification;

public class when_destroying_an_established_specification : given.a_specification_class_constructor
{
    // A probe per spec, as the shared instance is cached per class.
    sealed class Probe : recording_specification;

    Probe _specification;

    async Task Establish()
    {
        _specification = await Create<Probe>();
        await _specification.InitializeAsync();
        await _specification.InitializeAsync();
    }

    async Task Because()
    {
        await Specification.DestroyAsync(typeof(Probe));
        await Specification.DestroyAsync(typeof(Probe));
    }

    [Test] public void should_destroy_once() => _specification.Calls.Count(_ => _ == "Destroy").ShouldEqual(1);
    [Test] public void should_destroy_after_performing_the_action() => _specification.Calls.ShouldEqual("Establish", "Because", "Destroy");
}
