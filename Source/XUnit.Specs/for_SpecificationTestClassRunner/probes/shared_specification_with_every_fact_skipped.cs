// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_SpecificationTestClassRunner.probes;

#pragma warning disable xUnit1004 // The probe skips every fact on purpose.

// Internal so test discovery never finds it - it is only run through the harness.
sealed class shared_specification_with_every_fact_skipped : Specification
{
    internal static int establish_count;
    internal static int because_count;
    internal static int instances_created;

    public shared_specification_with_every_fact_skipped() => instances_created++;

    internal static void Reset() => establish_count = because_count = instances_created = 0;

    void Establish() => establish_count++;

    void Because() => because_count++;

    [Fact(Skip = "Skipped by the probe")]
    public void first_fact()
    {
    }

    [Fact(Skip = "Skipped by the probe")]
    public void second_fact()
    {
    }
}
