// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.for_Specification.probes;

namespace Cratis.Specifications.for_Specification;

public class when_initializing_a_specification_for_several_facts : given.a_specification_class_constructor
{
    // A probe per spec, as the shared instance is cached per class.
    sealed class Probe : recording_specification;

    Probe _specification;

    async Task Establish() => _specification = await Create<Probe>();

    async Task Because() => await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => _specification.InitializeAsync()));

    [Test] public void should_establish_once() => _specification.Calls.Count(_ => _ == "Establish").ShouldEqual(1);
    [Test] public void should_perform_the_action_once() => _specification.Calls.Count(_ => _ == "Because").ShouldEqual(1);
    [Test] public void should_establish_before_performing_the_action() => _specification.Calls.ShouldEqual("Establish", "Because");
}
