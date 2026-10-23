// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner;

/// <summary>
/// Represents the result of running a fact.
/// </summary>
/// <param name="Outcome">The outcome.</param>
/// <param name="Exception">The exception that failed the fact, if it failed.</param>
/// <param name="SkipReason">The reason the fact was skipped, if it was skipped.</param>
/// <param name="Started">When the fact started.</param>
/// <param name="Duration">How long the fact itself took; the shared context is not included.</param>
/// <param name="Steps">The timed phases of the class the fact ran in, when phase timings are on.</param>
internal readonly record struct FactResult(FactOutcome Outcome, Exception? Exception, string? SkipReason, DateTimeOffset Started, TimeSpan Duration, IReadOnlyList<PhaseRecord>? Steps = null)
{
    /// <summary>
    /// Creates a passed result.
    /// </summary>
    /// <param name="started">When the fact started.</param>
    /// <param name="duration">How long it took.</param>
    /// <returns>The result.</returns>
    public static FactResult Passed(DateTimeOffset started, TimeSpan duration) => new(FactOutcome.Passed, null, null, started, duration);

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    /// <param name="exception">The exception that failed the fact.</param>
    /// <param name="started">When the fact started.</param>
    /// <param name="duration">How long it took.</param>
    /// <returns>The result.</returns>
    public static FactResult Failed(Exception exception, DateTimeOffset started, TimeSpan duration) => new(FactOutcome.Failed, exception, null, started, duration);

    /// <summary>
    /// Creates a skipped result.
    /// </summary>
    /// <param name="reason">Why the fact was skipped.</param>
    /// <param name="started">When the fact would have started.</param>
    /// <returns>The result.</returns>
    public static FactResult Skipped(string reason, DateTimeOffset started) => new(FactOutcome.Skipped, null, reason, started, TimeSpan.Zero);
}
