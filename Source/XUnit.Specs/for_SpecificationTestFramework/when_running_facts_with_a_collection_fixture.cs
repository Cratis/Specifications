// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_SpecificationTestFramework;

[Collection("Fixture constructor specifications")]
public class when_running_facts_with_a_collection_fixture(the_fixture fixture) : Specification
{
    int _establishCount;
    void Establish() => _establishCount++;
    [Fact] void should_resolve_the_fixture() => fixture.ShouldNotBeNull();
    [Fact] void should_run_establish_once() => _establishCount.ShouldEqual(1);
}
