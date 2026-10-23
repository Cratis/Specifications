// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_SpecificationClassRunner;

public class when_run_contexts_overlap : Specification
{
    readonly TaskCompletionSource both_running = new(TaskCreationOptions.RunContinuationsAsynchronously);
    FactResult[][] results;
    int running;
    CancellationToken previous;
    CancellationToken restored;

    async Task Because()
    {
        previous = SpecificationContext.CancellationToken;
        using var first = new CancellationTokenSource();
        using var second = new CancellationTokenSource();
        results = await Task.WhenAll(Run(first.Token), Run(second.Token));
        restored = SpecificationContext.CancellationToken;
    }

    Task<FactResult[]> Run(CancellationToken token)
    {
        var facts = new[] { new FactDescriptor("check", "check", null, 0, _ => Check(token)) };
        var descriptor = new SpecificationClassDescriptor("context", "Context", "Context", "Context", () => new object(), null, null, null, facts);
        return SpecificationClassRunner.Run(new(descriptor, facts), sequentialFacts: true, token);
    }

    async ValueTask Check(CancellationToken token)
    {
        SpecificationContext.CancellationToken.ShouldEqual(token);
        if (Interlocked.Increment(ref running) == 2) both_running.SetResult();
        await both_running.Task;
        await Task.Yield();
        SpecificationContext.CancellationToken.ShouldEqual(token);
    }

    void should_isolate_tokens_in_overlapping_runs() => results.SelectMany(_ => _).ShouldEachConformTo(_ => _.Outcome == FactOutcome.Passed);

    void should_restore_the_parent_context() => restored.ShouldEqual(previous);
}
