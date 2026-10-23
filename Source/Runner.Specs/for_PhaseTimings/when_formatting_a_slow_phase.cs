// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_PhaseTimings;

public class when_formatting_a_slow_phase : Specification
{
    string plain;
    string with_resource_and_failure;

    void Because()
    {
        plain = SlowPhaseFormatter.Format(new(SpecificationPhase.Because, PhaseRecord.SpecOwner, "a spec", null, DateTimeOffset.UnixEpoch, TimeSpan.FromMilliseconds(1234.56), false));
        with_resource_and_failure = SlowPhaseFormatter.Format(new(SpecificationPhase.ResourceStart, PhaseRecord.KeyOwner, null, "database", DateTimeOffset.UnixEpoch, TimeSpan.FromMilliseconds(2000), true));
    }

    void should_name_the_phase_owner_duration_and_specification() => plain.ShouldEqual("Cratis.Specifications: slow Because (spec) took 1234.6 ms in a spec");
    void should_name_the_resource_and_the_failure() => with_resource_and_failure.ShouldEqual("Cratis.Specifications: slow ResourceStart (key) took 2000 ms for resource 'database' (failed)");
}
