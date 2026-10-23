// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.Runner.for_PhaseTimings.given;

namespace Cratis.Specifications.Runner.for_PhaseTimings;

public class when_timings_are_on : Specification
{
    const string Uid = "Cratis.Specifications.Runner.for_PhaseTimings.when_timings_are_on";

    readonly List<string> slow = [];
    IReadOnlyList<PhaseRecord> records;
    FactResult[] results;

    IEnumerable<string> Slow()
    {
        lock (slow) return [.. slow];
    }

    async Task Because()
    {
        var collector = PhaseTimings.Start(new PhaseTimingOptions(true, TimeSpan.Zero, null), line => { lock (slow) slow.Add(line); }, scope => scope.DisplayName == Uid);
        try
        {
            results = await SpecificationClassRunner.Run(timed_work.For(Uid), sequentialFacts: false);
            records = [.. collector.Records.Where(_ => _.Specification == Uid)];
        }
        finally
        {
            PhaseTimings.Stop(collector);
        }
    }

    void should_record_the_phases_in_order() => records.Select(_ => _.Phase).ShouldEqual(SpecificationPhase.Establish, SpecificationPhase.Because, SpecificationPhase.Destroy);
    void should_own_the_phases_by_the_specification() => records.ShouldEachConformTo(_ => _.Owner == PhaseRecord.SpecOwner);
    void should_not_mark_a_passing_phase_as_failed() => records.ShouldEachConformTo(_ => !_.Failed);
    void should_report_each_phase_above_the_threshold() => Slow().Count(_ => _.Contains(Uid)).ShouldEqual(3);
    void should_carry_the_steps_on_the_fact() => results[0].Steps.Select(_ => _.Phase).ShouldEqual(SpecificationPhase.Establish, SpecificationPhase.Because, SpecificationPhase.Destroy, SpecificationPhase.Fact);
}
