// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace Cratis.Specifications.Runner;

/// <summary>
/// Runs the facts of an established specification instance.
/// </summary>
/// <remarks>
/// Every fact is started in declaration order. A fact that completes synchronously is recorded immediately, so
/// synchronous facts cost no scheduling at all. Asynchronous facts keep running concurrently while the next facts
/// start and are awaited at the end, unless facts are to run sequentially.
/// </remarks>
internal static class FactRunner
{
    /// <summary>
    /// Runs the facts.
    /// </summary>
    /// <param name="instance">The established specification instance.</param>
    /// <param name="facts">The facts to run.</param>
    /// <param name="results">The results to fill in, in the order of the facts.</param>
    /// <param name="sequential">Whether to await each fact before starting the next.</param>
    /// <param name="started">When the class started.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task Run(object instance, IReadOnlyList<FactDescriptor> facts, FactResult[] results, bool sequential, DateTimeOffset started)
    {
        List<(int Index, long Started, ValueTask Task)>? pending = null;
        for (var index = 0; index < facts.Count; index++)
        {
            var fact = facts[index];
            if (fact.SkipReason is not null)
            {
                results[index] = FactResult.Skipped(fact.SkipReason, started);
                continue;
            }

            var timestamp = Stopwatch.GetTimestamp();
            ValueTask task;
            try
            {
                task = fact.Invoke(instance);
            }
            catch (Exception ex)
            {
                results[index] = FactResult.Failed(ex, started, Stopwatch.GetElapsedTime(timestamp));
                continue;
            }

            if (task.IsCompleted)
            {
                results[index] = ResultOfCompleted(task, started, timestamp);
            }
            else if (sequential)
            {
                results[index] = await CompleteAsync(task, started, timestamp);
            }
            else
            {
                (pending ??= []).Add((index, timestamp, task));
            }
        }

        if (pending is null) return;
        foreach (var (index, timestamp, task) in pending)
        {
            results[index] = await CompleteAsync(task, started, timestamp);
        }
    }

    static FactResult ResultOfCompleted(ValueTask task, DateTimeOffset started, long timestamp)
    {
        try
        {
            task.GetAwaiter().GetResult();
            return FactResult.Passed(started, Stopwatch.GetElapsedTime(timestamp));
        }
        catch (Exception ex)
        {
            return FactResult.Failed(ex, started, Stopwatch.GetElapsedTime(timestamp));
        }
    }

    static async ValueTask<FactResult> CompleteAsync(ValueTask task, DateTimeOffset started, long timestamp)
    {
        try
        {
            await task;
            return FactResult.Passed(started, Stopwatch.GetElapsedTime(timestamp));
        }
        catch (Exception ex)
        {
            return FactResult.Failed(ex, started, Stopwatch.GetElapsedTime(timestamp));
        }
    }
}
