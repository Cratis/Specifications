// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.Runner.for_PhaseTimings.given;

namespace Cratis.Specifications.Runner.for_PhaseTimings;

public class when_a_phase_is_below_the_threshold : Specification
{
    const string Uid = "Cratis.Specifications.Runner.for_PhaseTimings.when_a_phase_is_below_the_threshold";

    readonly List<string> slow = [];

    IEnumerable<string> Slow()
    {
        lock (slow) return [.. slow];
    }

    async Task Because()
    {
        var collector = PhaseTimings.Start(new PhaseTimingOptions(true, TimeSpan.FromHours(1), null), line => { lock (slow) slow.Add(line); }, scope => scope.DisplayName == Uid);
        try
        {
            await SpecificationClassRunner.Run(timed_work.For(Uid), sequentialFacts: false);
        }
        finally
        {
            PhaseTimings.Stop(collector);
        }
    }

    void should_not_report_it() => Slow().Count(_ => _.Contains(Uid)).ShouldEqual(0);
}
