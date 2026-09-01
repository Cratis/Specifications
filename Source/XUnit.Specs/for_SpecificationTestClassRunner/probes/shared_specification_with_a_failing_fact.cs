// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_SpecificationTestClassRunner.probes;

// Internal so test discovery never finds it - it is only run through the harness.
sealed class shared_specification_with_a_failing_fact : Specification
{
    internal static int establish_count;
    internal static int destroy_count;

    internal static void Reset() => establish_count = destroy_count = 0;

    void Establish() => establish_count++;

    void Destroy() => destroy_count++;

    [Fact]
    public void failing_fact() => throw new InvalidOperationException("deliberate failure");

    [Fact]
    public void passing_fact()
    {
    }

    [Fact]
    public void another_passing_fact()
    {
    }
}
