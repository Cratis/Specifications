// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Testing.Platform.Extensions.Messages;

namespace Cratis.Specifications.Runner.for_PhaseTimings;

public class when_creating_a_node_with_steps : Specification
{
    TestNode with_steps;
    TestNode without_steps;

    void Because()
    {
        var descriptor = given.timed_work.For("Cratis.Specifications.Runner.for_PhaseTimings.when_creating_a_node_with_steps").Specification;
        var fact = descriptor.Facts[0];
        var factory = new TestNodeFactory("Probes", new TrxCapability());
        var started = DateTimeOffset.UtcNow;
        var steps = new PhaseRecord[]
        {
            new(SpecificationPhase.Establish, PhaseRecord.SpecOwner, "a spec", null, started, TimeSpan.FromMilliseconds(5), false),
            new(SpecificationPhase.Fact, PhaseRecord.SpecOwner, "a spec", null, started, TimeSpan.FromMilliseconds(1), false)
        };
        with_steps = factory.Create(descriptor, fact, FactResult.Passed(started, TimeSpan.FromMilliseconds(1)) with { Steps = steps });
        without_steps = factory.Create(descriptor, fact, FactResult.Passed(started, TimeSpan.FromMilliseconds(1)));
    }

    void should_report_a_step_for_each_phase() => with_steps.Properties.Single<TimingProperty>().StepTimings.Select(_ => _.Id).ShouldEqual("Establish", "Fact");
    void should_report_the_duration_of_a_step() => with_steps.Properties.Single<TimingProperty>().StepTimings[0].Timing.Duration.ShouldEqual(TimeSpan.FromMilliseconds(5));
    void should_report_no_steps_without_phase_timings() => without_steps.Properties.Single<TimingProperty>().StepTimings.ShouldBeEmpty();
}
