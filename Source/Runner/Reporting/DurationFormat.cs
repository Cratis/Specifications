// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;

namespace Cratis.Specifications.Runner;

/// <summary>
/// Formats durations for people.
/// </summary>
internal static class DurationFormat
{
    /// <summary>
    /// Formats a duration as "&lt;1 ms", "12 ms" or "1.25 s".
    /// </summary>
    /// <param name="duration">The duration.</param>
    /// <returns>The text.</returns>
    public static string Format(TimeSpan duration)
    {
        if (duration < TimeSpan.FromMilliseconds(1)) return "<1 ms";
        return duration < TimeSpan.FromSeconds(1)
            ? $"{duration.TotalMilliseconds.ToString("0", CultureInfo.InvariantCulture)} ms"
            : $"{duration.TotalSeconds.ToString("0.00", CultureInfo.InvariantCulture)} s";
    }
}
