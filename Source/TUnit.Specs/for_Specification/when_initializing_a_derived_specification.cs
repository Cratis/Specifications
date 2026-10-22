// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.for_Specification.probes;

namespace Cratis.Specifications.for_Specification;

public class when_initializing_a_derived_specification : given.a_specification_class_constructor
{
    derived_recording_specification _specification;

    async Task Establish() => _specification = await Create<derived_recording_specification>();

    Task Because() => _specification.InitializeAsync();

    [Test] public void should_establish_from_the_base_type_towards_the_specific_type() => _specification.Calls.ShouldEqual("Establish", "Establish derived", "Because");
}
