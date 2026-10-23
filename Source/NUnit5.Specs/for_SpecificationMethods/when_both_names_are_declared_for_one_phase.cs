// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.for_SpecificationMethods.probes;

namespace Cratis.Specifications.for_SpecificationMethods;

public class when_both_names_are_declared_for_one_phase : Specification
{
    Exception _exception;
    Exception _synchronous;
    Task _phase;

    async Task Because()
    {
        _synchronous = Catch.Exception(() => { _phase = SpecificationMethods<probe_with_ambiguous_aliases, probe_root>.Establish(new()); });
        _exception = await Catch.Exception(() => _phase);
    }

    [Test] public void should_not_throw_synchronously() => _synchronous.ShouldBeNull();
    [Test] public void should_fail_with_the_ambiguity_exception() => _exception.ShouldBeOfExactType<AmbiguousLifecycleMethod>();
    [Test] public void should_identify_the_declaring_type_and_both_methods() => _exception.Message.ShouldEqual("probe_with_ambiguous_aliases declares both Establish and Given; use one of them.");
}
