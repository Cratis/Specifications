// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.for_SpecificationObservers.probes;
using Cratis.Specifications.Observation;

namespace Cratis.Specifications.for_SpecificationObservers;

public class when_observing_a_specification_that_skips_every_fact : for_SpecificationTestClassRunner.given.a_class_runner_harness
{
    IReadOnlyList<string> observed;

    async Task Because()
    {
        observed = (await RecordingObserver.Record<skipped_observed_specification>(() => RunClass(typeof(skipped_observed_specification)))).Events;
    }

    [Fact] void should_report_the_lifecycle_the_adapter_ran() => observed.ShouldEqual<string>(ObservationCatalog.Passing);
}
