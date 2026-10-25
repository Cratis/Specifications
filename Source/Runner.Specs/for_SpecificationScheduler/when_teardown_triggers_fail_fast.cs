// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_SpecificationScheduler;

public class when_teardown_triggers_fail_fast : Specification
{
    readonly given.a_recording_sink sink = new();
    IReadOnlyList<SpecificationWork> not_started;

    async Task Because()
    {
        var descriptor = given.the_probes.Get<failing_destroy>();
        not_started = await SpecificationScheduler.Run(
            [new(descriptor, descriptor.Facts), given.specification_work.For("queued")],
            SpecificationRunOptions.Default with { MaxParallelClasses = 1, FailFast = true }, sink, RunCache.Empty, CancellationToken.None);
    }

    void should_keep_the_fact_passing() => sink.Completed.Single().Results.Single().Outcome.ShouldEqual(FactOutcome.Passed);
    void should_report_the_separate_teardown_failure() => sink.TearDownFailures.Single().Outcome.ShouldEqual(FactOutcome.Failed);
    void should_not_start_the_queued_class() => not_started.Single().Specification.Uid.ShouldEqual("queued");
}
