// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.for_SpecificationObservers.given;
using Cratis.Specifications.Observation;

namespace Cratis.Specifications.for_SpecificationObservers;

// The fixture lifecycle NUnit drives: OneTimeSetUp, then OneTimeTearDown even when the set-up failed.
// NUnit runs neither for a fixture whose tests are all ignored, so such a fixture is never observed.
public class when_observing_the_lifecycle : Specification
{
    readonly Dictionary<ObservedFailure, IReadOnlyList<string>> observed = [];
    IReadOnlyList<SpecificationScope> current_during_phases;
    SpecificationScope current_after_the_lifecycle;

    async Task Because()
    {
        try
        {
            foreach (var failure in Enum.GetValues<ObservedFailure>())
            {
                ObservedBehavior.Failure = failure;
                var observer = await RecordingObserver.Record<observed_specification>(() => Run(new observed_specification()));
                observed[failure] = observer.Events;
                if (failure == ObservedFailure.None) current_during_phases = observer.CurrentDuringPhases;
            }
        }
        finally
        {
            ObservedBehavior.Failure = ObservedFailure.None;
        }

        current_after_the_lifecycle = SpecificationScope.Current;
    }

    [Test] public void should_report_a_passing_specification() => observed[ObservedFailure.None].ShouldEqual<string>(ObservationCatalog.Passing);
    [Test] public void should_report_a_failing_establish() => observed[ObservedFailure.Establish].ShouldEqual<string>(ObservationCatalog.EstablishFails);
    [Test] public void should_report_a_failing_because() => observed[ObservedFailure.Because].ShouldEqual<string>(ObservationCatalog.BecauseFails);
    [Test] public void should_report_a_failing_destroy() => observed[ObservedFailure.Destroy].ShouldEqual<string>(ObservationCatalog.DestroyFails);
    [Test] public void should_report_a_cancelled_because() => observed[ObservedFailure.Cancelled].ShouldEqual<string>(ObservationCatalog.Cancelled);
    [Test] public void should_flow_the_scope_through_every_phase() => current_during_phases.ShouldEachConformTo(_ => _ != null && _.Instance is observed_specification);
    [Test] public void should_not_leak_the_scope_to_the_caller() => current_after_the_lifecycle.ShouldBeNull();

    static async Task Run(observed_specification specification)
    {
        try
        {
            await specification.InitializeAsync();
        }
        catch (Exception)
        {
            // The catalog records the failure; NUnit would report it on the fixture.
        }

        try
        {
            await specification.DisposeAsync();
        }
        catch (Exception)
        {
            // As above.
        }
    }
}
