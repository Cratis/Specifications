// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Cratis.Specifications;

/// <summary>
/// Collects the timing of every phase reported through the observation seam, and reports the slow ones.
/// </summary>
/// <remarks>
/// It only exists when phase timings are on, so a run without them pays nothing for it. Reporting a slow phase is
/// additive: a failure of the reporting channel never changes the outcome of a specification.
/// </remarks>
/// <param name="slowThreshold">The duration above which a phase is reported as slow.</param>
/// <param name="reportSlow">Reports a line about a slow phase through the native channel of the framework, if it has one.</param>
/// <param name="observes">Selects the scopes to collect; all of them when null. Lets specifications collect their own scopes only.</param>
internal sealed class PhaseTimingCollector(TimeSpan slowThreshold, Action<string>? reportSlow, Func<SpecificationScope, bool>? observes = null) : ISpecificationObserver
{
    readonly Lock _lock = new();
    readonly List<PhaseRecord> _records = [];
#pragma warning disable IDE0028 // A ConditionalWeakTable has no collection expression form.
    readonly ConditionalWeakTable<SpecificationScope, List<PhaseRecord>> _byScope = new();
#pragma warning restore IDE0028

    /// <summary>
    /// Gets a snapshot of the records collected so far, in the order the phases completed.
    /// </summary>
    public IReadOnlyList<PhaseRecord> Records
    {
        get
        {
            lock (_lock) return [.. _records];
        }
    }

    /// <summary>
    /// Gets a snapshot of the records collected for a scope.
    /// </summary>
    /// <param name="scope">The scope.</param>
    /// <returns>The records, in the order the phases completed.</returns>
    public IReadOnlyList<PhaseRecord> RecordsFor(SpecificationScope scope)
    {
        lock (_lock) return _byScope.TryGetValue(scope, out var records) ? [.. records] : [];
    }

    /// <summary>
    /// Adds a record for a phase that does not belong to a specification scope, such as a shared resource or the session.
    /// </summary>
    /// <param name="record">The record.</param>
    public void Add(PhaseRecord record)
    {
        lock (_lock) _records.Add(record);
        ReportIfSlow(record);
    }

    /// <inheritdoc/>
    public ValueTask ScopeStarting(SpecificationScope scope) => default;

    /// <inheritdoc/>
    public void PhaseCompleted(SpecificationScope scope, SpecificationPhase phase, long startTimestamp, long endTimestamp, Exception? failure)
    {
        if (observes is not null && !observes(scope)) return;
        var duration = Stopwatch.GetElapsedTime(startTimestamp, endTimestamp);
        var record = new PhaseRecord(phase, PhaseRecord.SpecOwner, scope.DisplayName, null, DateTimeOffset.UtcNow - Stopwatch.GetElapsedTime(startTimestamp), duration, failure is not null);
        lock (_lock)
        {
            _records.Add(record);
            _byScope.GetOrCreateValue(scope).Add(record);
        }

        ReportIfSlow(record);
    }

    /// <inheritdoc/>
    public ValueTask ScopeEnded(SpecificationScope scope) => default;

    void ReportIfSlow(PhaseRecord record)
    {
        if (reportSlow is null || record.Duration <= slowThreshold) return;
        try
        {
            reportSlow(SlowPhaseFormatter.Format(record));
        }
        catch
        {
            // The report is additive; a channel that is not available must not change the outcome of a specification.
        }
    }
}
