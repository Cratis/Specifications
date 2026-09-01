// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_SpecificationTestClassRunner.probes;

// Internal so test discovery never finds it - it is only run through the harness.
sealed class plain_class_with_two_facts
{
    internal static int instances_created;

    public plain_class_with_two_facts() => instances_created++;

    internal static void Reset() => instances_created = 0;

    [Fact]
    public void first_fact()
    {
    }

    [Fact]
    public void second_fact()
    {
    }
}
