// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.for_SpecificationObservers.probes;

namespace Cratis.Specifications.for_PhaseTimings;

public class when_timing_a_specification : for_SpecificationTestClassRunner.given.a_class_runner_harness
{
    PhaseTimingCollector collector;
    PhaseTimingCollector off;

    async Task Because()
    {
        off = PhaseTimings.Start(new PhaseTimingOptions(false, TimeSpan.Zero, null), null);
        collector = PhaseTimings.Start(new PhaseTimingOptions(true, TimeSpan.FromHours(1), null), null, scope => scope.SpecificationType == typeof(disposable_observed_specification));
        try
        {
            await RunClass(typeof(disposable_observed_specification));
        }
        finally
        {
            PhaseTimings.Stop(collector);
        }
    }

    [Fact] void should_not_start_collecting_when_off() => off.ShouldBeNull();
    [Fact] void should_record_the_phases_in_order() => collector.Records.Select(_ => _.Phase).ShouldEqual(SpecificationPhase.Establish, SpecificationPhase.Because, SpecificationPhase.Destroy);
    [Fact] void should_own_the_phases_by_the_specification() => collector.Records.ShouldEachConformTo(_ => _.Owner == PhaseRecord.SpecOwner);
}
