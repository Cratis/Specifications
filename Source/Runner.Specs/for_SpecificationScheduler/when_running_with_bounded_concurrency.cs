// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_SpecificationScheduler;

public class when_running_with_bounded_concurrency : Specification
{
    int running;
    int most_running;
    given.a_recording_sink sink;
    IReadOnlyList<SpecificationWork> not_started;

    void Establish() => sink = new();

    async Task Because()
    {
        var plan = Enumerable.Range(0, 12).Select(_ => given.specification_work.For($"class{_}", because: Track)).ToList();
        not_started = await SpecificationScheduler.Run(plan, SpecificationRunOptions.Default with { MaxParallelClasses = 3 }, sink, RunCache.Empty, CancellationToken.None);
    }

    async ValueTask Track()
    {
        var now = Interlocked.Increment(ref running);
        InterlockedMax(now);
        await Task.Delay(10);
        Interlocked.Decrement(ref running);
    }

    void InterlockedMax(int value)
    {
        int current;
        while (value > (current = Volatile.Read(ref most_running)) && Interlocked.CompareExchange(ref most_running, value, current) != current)
        {
        }
    }

    void should_run_every_class() => sink.Completed.Count.ShouldEqual(12);

    void should_leave_nothing_unstarted() => not_started.ShouldBeEmpty();

    void should_never_run_more_classes_at_the_same_time_than_allowed() => most_running.ShouldBeLessThanOrEqual(3);

    void should_run_classes_concurrently() => most_running.ShouldBeGreaterThan(1);
}
