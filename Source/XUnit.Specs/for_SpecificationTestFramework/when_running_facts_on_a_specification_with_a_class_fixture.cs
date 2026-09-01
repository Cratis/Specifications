// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_SpecificationTestFramework;

// A Specification whose constructor takes a class fixture is not eligible for the shared
// lifecycle - it falls back to the stock per-fact lifecycle.
public class when_running_facts_on_a_specification_with_a_class_fixture(the_fixture fixture) : Specification, IClassFixture<the_fixture>
{
    internal static int establish_count;
    internal static int facts_run;

    void Establish() => establish_count++;

    [Fact]
    void should_resolve_the_fixture()
    {
        facts_run++;
        fixture.ShouldNotBeNull();
    }

    [Fact]
    void should_run_establish_for_the_first_fact()
    {
        facts_run++;
        establish_count.ShouldEqual(facts_run);
    }

    [Fact]
    void should_run_establish_for_the_second_fact()
    {
        facts_run++;
        establish_count.ShouldEqual(facts_run);
    }
}
