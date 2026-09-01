// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_SpecificationTestClassRunner.probes;

// Internal so test discovery never finds it - it is only run through the harness.
sealed class shared_specification_with_a_throwing_because : Specification
{
    internal static int establish_count;
    internal static int because_count;
    internal static int destroy_count;
    internal static int facts_executed;

    internal static void Reset() => establish_count = because_count = destroy_count = facts_executed = 0;

    void Establish() => establish_count++;

    void Because()
    {
        because_count++;
        throw new InvalidOperationException("deliberate Because failure");
    }

    void Destroy() => destroy_count++;

    [Fact]
    public void first_fact() => facts_executed++;

    [Fact]
    public void second_fact() => facts_executed++;
}
