// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_SpecificationTestClassRunner.probes;

// Internal so test discovery never finds it - it is only run through the harness.
sealed class shared_specification_with_three_facts : Specification
{
    internal static int establish_count;
    internal static int because_count;
    internal static int destroy_count;
    internal static int instances_created;
    internal static readonly List<string> lifecycle_order = [];

    public shared_specification_with_three_facts() => instances_created++;

    internal static void Reset()
    {
        establish_count = because_count = destroy_count = instances_created = 0;
        lifecycle_order.Clear();
    }

    void Establish()
    {
        establish_count++;
        lifecycle_order.Add("establish");
    }

    void Because()
    {
        because_count++;
        lifecycle_order.Add("because");
    }

    void Destroy()
    {
        destroy_count++;
        lifecycle_order.Add("destroy");
    }

    [Fact]
    public void first_fact() => lifecycle_order.Add("fact");

    [Fact]
    public void second_fact() => lifecycle_order.Add("fact");

    [Fact]
    public void third_fact() => lifecycle_order.Add("fact");
}
