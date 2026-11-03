// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_SpecificationTestClassRunner.probes;

#pragma warning disable xUnit1004 // The probe skips one fact on purpose.

// Internal so test discovery never finds it - it is only run through the harness.
sealed class shared_specification_with_one_skipped_fact : Specification
{
    public shared_specification_with_one_skipped_fact() => skipping_probe_counts.instances_created++;

    public static bool ConditionHolds => false;

    void Establish() => skipping_probe_counts.establish_count++;

    [Fact(Skip = "Probe")] public void skipped_fact() { }

    [Fact(Skip = "Probe", SkipWhen = nameof(ConditionHolds))] public void running_fact() { }
}
