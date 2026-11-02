// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_SpecificationTestClassRunner.probes;

// Internal so test discovery never finds it - it is only run through the harness.
sealed class shared_specification_skipped_when_a_condition_holds : Specification
{
    public shared_specification_skipped_when_a_condition_holds() => skipping_probe_counts.instances_created++;

    public static bool ConditionHolds => true;

    void Establish() => skipping_probe_counts.establish_count++;

    [Fact(Skip = "Probe", SkipWhen = nameof(ConditionHolds))] public void first_fact() { }

    [Fact(Skip = "Probe", SkipWhen = nameof(ConditionHolds))] public void second_fact() { }
}
