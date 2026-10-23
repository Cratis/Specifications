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
    /// <param name="cancellationToken">The cooperative cancellation token of the run.</param>
    /// <returns>The result of every selected fact, in order.</returns>
    public static async Task<FactResult[]> Run(SpecificationWork work, bool sequentialFacts, CancellationToken cancellationToken = default)
    {
        var previous = SpecificationContext.CancellationToken;
        SpecificationContext.CancellationToken = cancellationToken;
        try
        {
            return await RunCore(work, sequentialFacts);
        }
        finally
        {
            SpecificationContext.CancellationToken = previous;
        }
    }

    static async Task<FactResult[]> RunCore(SpecificationWork work, bool sequentialFacts)
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
        SpecificationScope? scope = null;
        Exception? contextFailure = null;
        try
        {
            instance = specification.Create();
            scope = await StartScope(instance, specification.DisplayName);
            await RunPhase(scope, SpecificationPhase.Establish, specification.Establish, instance);
            await RunPhase(scope, SpecificationPhase.Because, specification.Because, instance);
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

        if (instance is not null && await TearDown(specification, instance, scope) is Exception teardownFailure)
        {
            ApplyTearDownFailure(results, teardownFailure);
        }

        return results;
    }

    static ValueTask RunPhase(SpecificationScope? scope, SpecificationPhase phase, Func<object, ValueTask>? body, object instance)
    {
        if (scope is not null) return scope.Run(phase, body);
        return body is null ? default : body(instance);
    }

    static async ValueTask<SpecificationScope?> StartScope(object instance, string displayName)
    {
        var scope = SpecificationScope.CreateIfObserved(instance, displayName);
        if (scope is not null) await scope.Start();
        return scope;
    }

    static async ValueTask<Exception?> TearDown(SpecificationClassDescriptor specification, object instance, SpecificationScope? scope)
    {
        var failure = await Attempt(() => RunPhase(scope, SpecificationPhase.Destroy, specification.Destroy, instance));
        failure = Combine(failure, await Attempt(() => Dispose(instance)));
        if (scope is null) return failure;
        return Combine(failure, await Attempt(scope.End));
    }

    static async ValueTask Dispose(object instance)
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

    static async ValueTask<Exception?> Attempt(Func<ValueTask> action)
    {
        try
        {
            await action();
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    static Exception? Combine(Exception? failure, Exception? next) =>
        failure is null ? next : next is null ? failure : new AggregateException(failure, next);

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

    static void ApplyTearDownFailure(FactResult[] results, Exception exception)
    {
        for (var index = 0; index < results.Length; index++)
        {
            var result = results[index];
            if (result.Outcome == FactOutcome.Skipped) continue;
            var failure = result.Exception is null ? exception : new AggregateException(result.Exception, exception);
            results[index] = FactResult.Failed(failure, result.Started, result.Duration);
        }
    }
}
