// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.for_SpecificationObservers.probes;
using Cratis.Specifications.Observation;

namespace Cratis.Specifications.for_SpecificationObservers;

public class when_observing_the_lifecycle : for_SpecificationTestClassRunner.given.a_class_runner_harness
{
    readonly Dictionary<ObservedFailure, IReadOnlyList<string>> observed = [];
    IReadOnlyList<string> per_fact;
    IReadOnlyList<string> unobserved;
    IReadOnlyList<SpecificationScope> current_during_phases;
    bool disposed_before_scope_ended;

    async Task Because()
    {
        try
        {
            foreach (var failure in Enum.GetValues<ObservedFailure>())
            {
                ObservedBehavior.Failure = failure;
                var observer = await RecordingObserver.Record<observed_specification>(() => RunClass(typeof(observed_specification)));
                observed[failure] = observer.Events;
                if (failure == ObservedFailure.None) current_during_phases = observer.CurrentDuringPhases;
            }
        }
        finally
        {
            ObservedBehavior.Failure = ObservedFailure.None;
        }

        per_fact = (await RecordingObserver.Record<per_fact_observed_specification>(() => RunClass(typeof(per_fact_observed_specification)))).Events;

        // The filter runs for every report, the last one being the end of the scope.
        disposable_observed_specification.disposed_before_scope_ended = false;
        await RecordingObserver.Record(
            scope =>
            {
                if (scope.Instance is disposable_observed_specification) disposed_before_scope_ended = disposable_observed_specification.disposed_before_scope_ended;
                return false;
            },
            () => RunClass(typeof(disposable_observed_specification)));

        var removed = await RecordingObserver.Record<observed_specification>(() => Task.CompletedTask);
        await RunClass(typeof(observed_specification));
        unobserved = removed.Events;
    }

    [Fact] void should_report_a_passing_specification() => observed[ObservedFailure.None].ShouldEqual<string>(ObservationCatalog.Passing);
    [Fact] void should_report_a_failing_establish() => observed[ObservedFailure.Establish].ShouldEqual<string>(ObservationCatalog.EstablishFails);
    [Fact] void should_report_a_failing_because() => observed[ObservedFailure.Because].ShouldEqual<string>(ObservationCatalog.BecauseFails);
    [Fact] void should_report_a_failing_destroy() => observed[ObservedFailure.Destroy].ShouldEqual<string>(ObservationCatalog.DestroyFails);
    [Fact] void should_report_a_cancelled_because() => observed[ObservedFailure.Cancelled].ShouldEqual<string>(ObservationCatalog.Cancelled);
    [Fact] void should_report_every_instance_of_a_per_fact_specification() => per_fact.ShouldEqual<string>(ObservationCatalog.PassingTimes(2));
    [Fact] void should_flow_the_scope_through_every_phase() => current_during_phases.ShouldEachConformTo(_ => _ != null && _.Instance is observed_specification);
    [Fact] void should_end_the_scope_after_disposing_a_shared_instance() => disposed_before_scope_ended.ShouldBeTrue();
    [Fact] void should_report_nothing_once_the_observer_is_removed() => unobserved.ShouldBeEmpty();
}
