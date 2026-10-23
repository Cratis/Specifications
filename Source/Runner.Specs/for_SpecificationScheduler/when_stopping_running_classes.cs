// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_SpecificationScheduler;

public class when_stopping_running_classes : Specification
{
    readonly TaskCompletionSource both_running = new(TaskCreationOptions.RunContinuationsAsynchronously);
    readonly System.Collections.Concurrent.ConcurrentQueue<string> calls = new();
    CancellationToken run_token;
    Func<Task> cancel;
    given.a_recording_sink sink;
    IReadOnlyList<SpecificationWork> not_started;
    int running;
    CancellationToken outer_token;

    void Establish()
    {
        outer_token = SpecificationContext.CancellationToken;
        sink = new();
    }

    async Task Because()
    {
        using var cancellation = new CancellationTokenSource();
        run_token = cancellation.Token;
        cancel = cancellation.CancelAsync;
        var first = Work("first");
        var second = Work("second");
        not_started = await SpecificationScheduler.Run(
            [first, second, .. given.specification_work.Many(10)],
            SpecificationRunOptions.Default with { MaxParallelClasses = 2, SequentialFacts = true }, sink, RunCache.Empty, run_token);
    }

    SpecificationWork Work(string uid)
    {
        var facts = new[]
        {
            new FactDescriptor("first", "first", null, 0, _ => Record(uid + " first")),
            new FactDescriptor("second", "second", null, 0, _ => Record(uid + " second"))
        };
        var descriptor = new SpecificationClassDescriptor(uid, "Stopping", uid, uid,
            () => new disposable_context(() => Record(uid + " dispose")),
            _ => Record(uid + " establish"), _ => WaitForStop(), _ => Record(uid + " destroy"), facts);
        return new(descriptor, descriptor.Facts);
    }

    async ValueTask WaitForStop()
    {
        if (Interlocked.Increment(ref running) == 2)
        {
            await cancel();
            both_running.SetResult();
        }

        await both_running.Task;
        SpecificationContext.CancellationToken.ShouldEqual(run_token);
    }

    ValueTask Record(string call)
    {
        SpecificationContext.CancellationToken.ShouldEqual(run_token);
        calls.Enqueue(call);
        return ValueTask.CompletedTask;
    }

    void should_not_schedule_another_class() => not_started.Count.ShouldEqual(10);

    void should_complete_each_running_class_exactly_once() => sink.Completed.Count.ShouldEqual(2);

    void should_finish_all_facts_destroy_and_disposal() => calls.Count.ShouldEqual(10);

    void should_finish_the_first_class_in_lifecycle_order() => calls.Where(_ => _.StartsWith("first ", StringComparison.Ordinal)).ShouldEqual(
        "first establish", "first first", "first second", "first destroy", "first dispose");

    void should_finish_the_second_class_in_lifecycle_order() => calls.Where(_ => _.StartsWith("second ", StringComparison.Ordinal)).ShouldEqual(
        "second establish", "second first", "second second", "second destroy", "second dispose");

    void should_pass_every_fact() => sink.Completed.SelectMany(_ => _.Results).ShouldEachConformTo(_ => _.Outcome == FactOutcome.Passed);

    void should_restore_the_callers_context() => SpecificationContext.CancellationToken.ShouldEqual(outer_token);

    sealed class disposable_context(Func<ValueTask> dispose) : IDisposable
    {
        public void Dispose()
        {
            var result = dispose();
            if (!result.IsCompleted) throw new InvalidOperationException("The disposal probe must complete synchronously.");
            result.GetAwaiter().GetResult();
        }
    }
}
