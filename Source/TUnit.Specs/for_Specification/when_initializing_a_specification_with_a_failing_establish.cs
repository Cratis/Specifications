// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.for_Specification.probes;

namespace Cratis.Specifications.for_Specification;

public class when_initializing_a_specification_with_a_failing_establish : given.a_specification_class_constructor
{
    failing_establish_specification _specification;
    Exception _firstFact;
    Exception _secondFact;

    async Task Establish() => _specification = await Create<failing_establish_specification>();

    async Task Because()
    {
        _firstFact = await Catch.Exception(() => _specification.InitializeAsync());
        _secondFact = await Catch.Exception(() => _specification.InitializeAsync());
    }

    [Test] public void should_fail_the_first_fact() => _firstFact.ShouldNotBeNull();
    [Test] public void should_fail_every_other_fact() => _secondFact.ShouldNotBeNull();
    [Test] public void should_fail_with_the_reason_from_establish() => _secondFact.ToString().ShouldContain("Establish failed");
    [Test] public void should_only_establish_once() => _specification.EstablishCalls.ShouldEqual(1);
}
