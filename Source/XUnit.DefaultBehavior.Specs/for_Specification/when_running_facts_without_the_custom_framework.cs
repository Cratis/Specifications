// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_Specification;

// This assembly does NOT opt in to SpecificationTestFramework - stock xUnit behavior must be
// unchanged: a fresh instance per fact, with Establish/Because before and Destroy after every fact.
public class when_running_facts_without_the_custom_framework : Specification
{
    internal static int establish_count;
    internal static int because_count;
    internal static int destroy_count;
    internal static int instances_created;
    internal static int facts_run;

    public when_running_facts_without_the_custom_framework() => instances_created++;

    void Establish() => establish_count++;

    void Because() => because_count++;

    void Destroy() => destroy_count++;

    [Fact]
    void should_run_the_full_lifecycle_for_the_first_fact() => AssertPerFactLifecycle();

    [Fact]
    void should_run_the_full_lifecycle_for_the_second_fact() => AssertPerFactLifecycle();

    [Fact]
    void should_run_the_full_lifecycle_for_the_third_fact() => AssertPerFactLifecycle();

    static void AssertPerFactLifecycle()
    {
        facts_run++;
        instances_created.ShouldEqual(facts_run);
        establish_count.ShouldEqual(facts_run);
        because_count.ShouldEqual(facts_run);
        destroy_count.ShouldEqual(facts_run - 1);
    }
}
