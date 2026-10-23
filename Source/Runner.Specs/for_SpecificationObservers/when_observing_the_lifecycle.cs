// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.Observation;
using Cratis.Specifications.Runner.for_SpecificationObservers.given;

namespace Cratis.Specifications.Runner.for_SpecificationObservers;

public class when_observing_the_lifecycle : Specification
{
    readonly Dictionary<ObservedFailure, IReadOnlyList<string>> observed = [];
    readonly Dictionary<ObservedFailure, FactResult[]> results = [];
    IReadOnlyList<string> skipped;
    IReadOnlyList<SpecificationScope> current_during_phases;
    bool disposed_before_scope_ended;

    async Task Because()
    {
        try
        {
            foreach (var failure in Enum.GetValues<ObservedFailure>())
            {
                ObservedBehavior.Failure = failure;
                var uid = $"{typeof(when_observing_the_lifecycle).FullName}.{failure}";
                var observer = await RecordingObserver.Record(_ => _.DisplayName == uid, async () => results[failure] = await SpecificationClassRunner.Run(observed_work.For(uid), sequentialFacts: false));
                observed[failure] = observer.Events;
                if (failure == ObservedFailure.None) current_during_phases = observer.CurrentDuringPhases;
            }
        }
        finally
        {
            ObservedBehavior.Failure = ObservedFailure.None;
        }

        var skippedUid = $"{typeof(when_observing_the_lifecycle).FullName}.skipped";
        skipped = (await RecordingObserver.Record(_ => _.DisplayName == skippedUid, () => SpecificationClassRunner.Run(observed_work.For(skippedUid, skipped: true), sequentialFacts: false))).Events;

        // The filter runs for every report, the last one being the end of the scope. The class runner disposes the instance.
        using var instance = new disposable_instance();
        await RecordingObserver.Record(
            scope =>
            {
                if (ReferenceEquals(scope.Instance, instance)) disposed_before_scope_ended = instance.Disposed;
                return false;
            },
            () => SpecificationClassRunner.Run(observed_work.For($"{typeof(when_observing_the_lifecycle).FullName}.disposable", create: () => instance), sequentialFacts: false));
    }

    void should_report_a_passing_specification() => observed[ObservedFailure.None].ShouldEqual<string>(ObservationCatalog.Passing);
    void should_report_a_failing_establish() => observed[ObservedFailure.Establish].ShouldEqual<string>(ObservationCatalog.EstablishFails);
    void should_report_a_failing_because() => observed[ObservedFailure.Because].ShouldEqual<string>(ObservationCatalog.BecauseFails);
    void should_report_a_failing_destroy() => observed[ObservedFailure.Destroy].ShouldEqual<string>(ObservationCatalog.DestroyFails);
    void should_report_a_cancelled_because() => observed[ObservedFailure.Cancelled].ShouldEqual<string>(ObservationCatalog.Cancelled);
    void should_report_nothing_for_a_class_that_skips_every_fact() => skipped.ShouldEqual<string>(ObservationCatalog.AllSkipped);
    void should_flow_the_scope_through_every_phase() => current_during_phases.ShouldEachConformTo(_ => _ != null);
    void should_end_the_scope_after_disposal() => disposed_before_scope_ended.ShouldBeTrue();
    void should_keep_passing_facts_passing() => results[ObservedFailure.None].ShouldEachConformTo(_ => _.Outcome == FactOutcome.Passed);
    void should_fail_facts_on_a_failing_destroy() => results[ObservedFailure.Destroy].ShouldEachConformTo(_ => _.Outcome == FactOutcome.Failed);
}
