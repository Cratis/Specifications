// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner;

/// <summary>
/// Runs one specification class: one instance, Establish and Because once, every selected fact against that
/// shared context, then Destroy and disposal once.
/// </summary>
internal static class SpecificationClassRunner
{
    /// <summary>
    /// Runs the given work.
    /// </summary>
    /// <param name="work">The specification class and its selected facts.</param>
    /// <param name="sequentialFacts">Whether to await each fact before starting the next.</param>
    /// <returns>The result of every selected fact, in order.</returns>
    public static async Task<FactResult[]> Run(SpecificationWork work, bool sequentialFacts)
    {
        var specification = work.Specification;
        var facts = work.Facts;
        var results = new FactResult[facts.Count];
        var started = DateTimeOffset.UtcNow;

        if (specification.Error is not null || specification.Create is null)
        {
            FailAll(facts, results, new SpecificationDiscoveryException(specification.Error ?? "The specification class can not be created."), started);
            return results;
        }

        if (AllSkipped(facts))
        {
            FailAll(facts, results, null, started);
            return results;
        }

        object? instance = null;
        Exception? contextFailure = null;
        try
        {
            instance = specification.Create();
            await RunPhase(specification.Establish, instance);
            await RunPhase(specification.Because, instance);
        }
        catch (Exception ex)
        {
            contextFailure = ex;
        }

        if (contextFailure is null)
        {
            await FactRunner.Run(instance!, facts, results, sequentialFacts, started);
        }
        else
        {
            FailAll(facts, results, contextFailure, started);
        }

        if (instance is not null && await TearDown(specification, instance) is Exception teardownFailure)
        {
            FailPassed(results, teardownFailure);
        }

        return results;
    }

    static ValueTask RunPhase(Func<object, ValueTask>? phase, object instance) => phase is null ? default : phase(instance);

    static async ValueTask<Exception?> TearDown(SpecificationClassDescriptor specification, object instance)
    {
        Exception? failure = null;
        try
        {
            await RunPhase(specification.Destroy, instance);
        }
        catch (Exception ex)
        {
            failure = ex;
        }

        try
        {
            if (instance is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
            else if (instance is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        catch (Exception ex)
        {
            failure ??= ex;
        }

        return failure;
    }

    static bool AllSkipped(IReadOnlyList<FactDescriptor> facts)
    {
        for (var index = 0; index < facts.Count; index++)
        {
            if (facts[index].SkipReason is null) return false;
        }

        return true;
    }

    static void FailAll(IReadOnlyList<FactDescriptor> facts, FactResult[] results, Exception? exception, DateTimeOffset started)
    {
        for (var index = 0; index < facts.Count; index++)
        {
            results[index] = facts[index].SkipReason is not null || exception is null
                ? FactResult.Skipped(facts[index].SkipReason ?? string.Empty, started)
                : FactResult.Failed(exception, started, TimeSpan.Zero);
        }
    }

    static void FailPassed(FactResult[] results, Exception exception)
    {
        for (var index = 0; index < results.Length; index++)
        {
            if (results[index].Outcome == FactOutcome.Passed)
            {
                results[index] = FactResult.Failed(exception, results[index].Started, results[index].Duration);
            }
        }
    }
}
