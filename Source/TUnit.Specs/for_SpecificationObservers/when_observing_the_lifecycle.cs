// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.for_SpecificationObservers.probes;
using Cratis.Specifications.Observation;

namespace Cratis.Specifications.for_SpecificationObservers;

// The lifecycle TUnit drives: the class constructor, the before-test hook, then the after-class hook.
public class when_observing_the_lifecycle : for_Specification.given.a_specification_class_constructor
{
    sealed class passing_probe : observed_specification;
    sealed class failing_establish_probe : observed_specification;
    sealed class failing_because_probe : observed_specification;
    sealed class failing_destroy_probe : observed_specification;
    sealed class cancelled_probe : observed_specification;

    static readonly Dictionary<ObservedFailure, Type> probes = new()
    {
        [ObservedFailure.None] = typeof(passing_probe),
        [ObservedFailure.Establish] = typeof(failing_establish_probe),
        [ObservedFailure.Because] = typeof(failing_because_probe),
        [ObservedFailure.Destroy] = typeof(failing_destroy_probe),
        [ObservedFailure.Cancelled] = typeof(cancelled_probe)
    };

    readonly Dictionary<ObservedFailure, IReadOnlyList<string>> observed = [];
    IReadOnlyList<SpecificationScope> current_during_phases;

    async Task Because()
    {
        try
        {
            foreach (var (failure, type) in probes)
            {
                ObservedBehavior.Failure = failure;
                var observer = await RecordingObserver.Record(_ => _.SpecificationType == type, () => Run(type));
                observed[failure] = observer.Events;
                if (failure == ObservedFailure.None) current_during_phases = observer.CurrentDuringPhases;
            }
        }
        finally
        {
            ObservedBehavior.Failure = ObservedFailure.None;
        }
    }

    [Test] public void should_report_a_passing_specification() => observed[ObservedFailure.None].ShouldEqual<string>(ObservationCatalog.Passing);
    [Test] public void should_report_a_failing_establish() => observed[ObservedFailure.Establish].ShouldEqual<string>(ObservationCatalog.EstablishFails);
    [Test] public void should_report_a_failing_because() => observed[ObservedFailure.Because].ShouldEqual<string>(ObservationCatalog.BecauseFails);
    [Test] public void should_report_a_failing_destroy() => observed[ObservedFailure.Destroy].ShouldEqual<string>(ObservationCatalog.DestroyFails);
    [Test] public void should_report_a_cancelled_because() => observed[ObservedFailure.Cancelled].ShouldEqual<string>(ObservationCatalog.Cancelled);
    [Test] public void should_flow_the_scope_through_every_phase() => current_during_phases.ShouldEachConformTo(_ => _ != null && _.Instance is passing_probe);

    async Task Run(Type type)
    {
        var specification = (Specification)await constructor.Create(type, null);
        try
        {
            await specification.InitializeAsync();
        }
        catch (Exception)
        {
            // The catalog records the failure; TUnit would report it on every test of the class.
        }

        try
        {
            await Specification.DestroyAsync(type);
        }
        catch (Exception)
        {
            // As above.
        }
    }
}
