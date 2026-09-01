// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_SpecificationTestFramework;

// Not a Specification - the custom framework must leave plain xUnit classes on the stock
// per-fact lifecycle, constructing a fresh instance for every fact.
public class when_running_facts_on_a_plain_xunit_class
{
    internal static int instances_created;
    internal static int facts_run;

    public when_running_facts_on_a_plain_xunit_class() => instances_created++;

    [Fact]
    void should_get_a_fresh_instance_for_the_first_fact()
    {
        facts_run++;
        instances_created.ShouldEqual(facts_run);
    }

    [Fact]
    void should_get_a_fresh_instance_for_the_second_fact()
    {
        facts_run++;
        instances_created.ShouldEqual(facts_run);
    }
}
