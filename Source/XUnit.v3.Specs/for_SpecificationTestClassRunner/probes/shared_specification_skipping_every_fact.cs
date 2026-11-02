// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_SpecificationTestClassRunner.probes;

#pragma warning disable xUnit1004 // The probe skips every fact on purpose.

// Internal so test discovery never finds it - it is only run through the harness.
sealed class shared_specification_skipping_every_fact : Specification
{
    public shared_specification_skipping_every_fact() => skipping_probe_counts.instances_created++;

    void Establish() => skipping_probe_counts.establish_count++;

    [Fact(Skip = "Probe")] public void first_fact() { }

    [Fact(Skip = "Probe")] public void second_fact() { }
}
