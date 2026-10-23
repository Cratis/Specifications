// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;

namespace Cratis.Specifications;

/// <summary>
/// Formats the line reported for a phase that took longer than the slow phase threshold.
/// </summary>
internal static class SlowPhaseFormatter
{
    /// <summary>
    /// Formats a phase record.
    /// </summary>
    /// <param name="record">The record.</param>
    /// <returns>The line to report.</returns>
    public static string Format(PhaseRecord record)
    {
        var milliseconds = record.Duration.TotalMilliseconds.ToString("0.#", CultureInfo.InvariantCulture);
        var line = $"Cratis.Specifications: slow {record.Phase} ({record.Owner}) took {milliseconds} ms";
        if (record.ResourceKey is not null) line += $" for resource '{record.ResourceKey}'";
        if (record.Specification is not null) line += $" in {record.Specification}";
        return record.Failed ? $"{line} (failed)" : line;
    }
}
