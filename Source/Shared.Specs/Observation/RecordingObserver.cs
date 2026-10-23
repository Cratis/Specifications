// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Observation;

/// <summary>
/// Records the lifecycle events reported for the scopes it observes, and unregisters itself when disposed.
/// </summary>
/// <remarks>
/// Other specifications run while the observer is registered, so it only records the scopes the filter selects.
/// </remarks>
internal sealed class RecordingObserver : ISpecificationObserver, IDisposable
{
    readonly Func<SpecificationScope, bool> _observes;
    readonly List<string> _events = [];

    public RecordingObserver(Func<SpecificationScope, bool> observes)
    {
        _observes = observes;
        SpecificationObservers.Add(this);
    }

    public IReadOnlyList<string> Events
    {
        get
        {
            lock (_events) return [.. _events];
        }
    }

    public List<SpecificationScope> CurrentDuringPhases { get; } = [];

    /// <summary>
    /// Records the scopes the filter selects while running the given code, and unregisters afterwards.
    /// </summary>
    /// <param name="observes">The filter selecting the scopes to record.</param>
    /// <param name="run">The code to run while recording.</param>
    /// <returns>The observer, holding what it recorded.</returns>
    public static async Task<RecordingObserver> Record(Func<SpecificationScope, bool> observes, Func<Task> run)
    {
        var observer = new RecordingObserver(observes);
        try
        {
            await run();
        }
        finally
        {
            observer.Dispose();
        }

        return observer;
    }

    /// <summary>
    /// Records the scopes of a specification type while running the given code, and unregisters afterwards.
    /// </summary>
    /// <typeparam name="TSpecification">The specification type to record.</typeparam>
    /// <param name="run">The code to run while recording.</param>
    /// <returns>The observer, holding what it recorded.</returns>
    public static Task<RecordingObserver> Record<TSpecification>(Func<Task> run) => Record(_ => _.SpecificationType == typeof(TSpecification), run);

    public ValueTask ScopeStarting(SpecificationScope scope)
    {
        Record(scope, ObservationCatalog.Starting);
        return ValueTask.CompletedTask;
    }

    public void PhaseCompleted(SpecificationScope scope, SpecificationPhase phase, long startTimestamp, long endTimestamp, Exception failure)
    {
        if (!_observes(scope)) return;
        lock (_events) CurrentDuringPhases.Add(SpecificationScope.Current);
        Record(scope, failure is null ? phase.ToString() : $"{phase} failed");
    }

    public ValueTask ScopeEnded(SpecificationScope scope)
    {
        Record(scope, ObservationCatalog.Ended);
        return ValueTask.CompletedTask;
    }

    public void Dispose() => SpecificationObservers.Remove(this);

    void Record(SpecificationScope scope, string entry)
    {
        if (!_observes(scope)) return;
        lock (_events) _events.Add(entry);
    }
}
