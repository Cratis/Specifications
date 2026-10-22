// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace Cratis.Specifications.Runner;

/// <summary>
/// Runs planned specification classes with bounded concurrency.
/// </summary>
/// <remarks>
/// A fixed number of workers take the next class from the plan, so the order of the plan is the start order and
/// there is no per-class task or semaphore overhead. Cancellation and fail-fast stop new classes from starting;
/// classes already running complete so that Destroy and disposal always run.
/// </remarks>
internal static class SpecificationScheduler
{
    /// <summary>
    /// Runs the plan.
    /// </summary>
    /// <param name="plan">The work, in the order to start it.</param>
    /// <param name="options">The run options.</param>
    /// <param name="sink">The receiver of the results.</param>
    /// <param name="cache">The cache recording durations and failures for the next run.</param>
    /// <param name="cancellationToken">The token cancelling the run.</param>
    /// <returns>The work that was not started, because the run was cancelled or stopped.</returns>
    public static async Task<IReadOnlyList<SpecificationWork>> Run(
        IReadOnlyList<SpecificationWork> plan,
        SpecificationRunOptions options,
        ISpecificationResultSink sink,
        RunCache cache,
        CancellationToken cancellationToken)
    {
        var control = new ExecutionControl();
        var next = -1;
        var workers = Math.Clamp(options.MaxParallelClasses, 1, Math.Max(1, plan.Count));
        var tasks = new Task[workers];
        for (var worker = 0; worker < workers; worker++)
        {
            tasks[worker] = Task.Run(Work, CancellationToken.None);
        }

        await Task.WhenAll(tasks);
        var notStarted = Math.Min(plan.Count, Volatile.Read(ref next) + 1);
        return notStarted >= plan.Count ? [] : [.. plan.Skip(notStarted)];

        async Task Work()
        {
            while (!cancellationToken.IsCancellationRequested && !control.StopRequested)
            {
                var index = Interlocked.Increment(ref next);
                if (index >= plan.Count) return;
                var work = plan[index];
                await sink.Starting(work);
                var timestamp = Stopwatch.GetTimestamp();
                var results = await SpecificationClassRunner.Run(work, options.SequentialFacts);
                var failed = AnyFailed(results);
                cache.Record(work.Specification.Uid, Stopwatch.GetElapsedTime(timestamp), failed);
                await sink.Completed(work, results);
                if (failed && options.FailFast) control.RequestStop();
            }
        }
    }

    static bool AnyFailed(FactResult[] results)
    {
        foreach (var result in results)
        {
            if (result.Outcome == FactOutcome.Failed) return true;
        }

        return false;
    }
}
