// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_SpecificationScheduler;

public class when_the_run_is_cancelled : Specification
{
    given.a_recording_sink sink;
    IReadOnlyList<SpecificationWork> not_started;
    CancellationTokenSource cancellation;

    void Establish()
    {
        sink = new();
        cancellation = new();
    }

    async Task Because()
    {
        var plan = new[] { given.specification_work.For("cancelling", because: Cancel) }.Concat(given.specification_work.Many(10)).ToList();
        not_started = await SpecificationScheduler.Run(plan, SpecificationRunOptions.Default with { MaxParallelClasses = 1 }, sink, RunCache.Empty, cancellation.Token);
    }

    async ValueTask Cancel() => await cancellation.CancelAsync();

    void Destroy() => cancellation.Dispose();

    void should_complete_the_class_that_was_running() => sink.Completed.Select(_ => _.Work.Specification.Uid).ShouldContainOnly("cancelling");

    void should_not_start_any_further_class() => not_started.Count.ShouldEqual(10);
}
