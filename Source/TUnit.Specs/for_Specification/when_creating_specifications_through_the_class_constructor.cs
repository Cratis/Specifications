// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.for_Specification.probes;

namespace Cratis.Specifications.for_Specification;

public class when_creating_specifications_through_the_class_constructor : given.a_specification_class_constructor
{
    // A probe per spec, as the shared instance is cached per class.
    sealed class Probe : recording_specification;

    Probe _first;
    Probe _second;
    derived_recording_specification _other;

    async Task Because()
    {
        _first = await Create<Probe>();
        _second = await Create<Probe>();
        _other = await Create<derived_recording_specification>();
    }

    [Test] public void should_share_the_instance_for_the_same_class() => _second.ShouldBeSame(_first);
    [Test] public void should_create_separate_instances_for_different_classes() => _other.ShouldNotBeSame(_first);
}
