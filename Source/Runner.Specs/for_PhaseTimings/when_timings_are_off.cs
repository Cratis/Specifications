// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.Runner.for_PhaseTimings.given;

namespace Cratis.Specifications.Runner.for_PhaseTimings;

public class when_timings_are_off : Specification
{
    const string Uid = "Cratis.Specifications.Runner.for_PhaseTimings.when_timings_are_off";

    PhaseTimingCollector started;
    FactResult[] results;

    async Task Because()
    {
        started = PhaseTimings.Start(new PhaseTimingOptions(false, TimeSpan.Zero, null), _ => { });
        results = (await SpecificationClassRunner.Run(timed_work.For(Uid), sequentialFacts: false)).Facts;
    }

    void should_not_start_collecting() => started.ShouldBeNull();
    void should_not_have_a_collector() => PhaseTimings.Collector.ShouldBeNull();
    void should_not_attach_steps_to_the_facts() => results[0].Steps.ShouldBeNull();
}
