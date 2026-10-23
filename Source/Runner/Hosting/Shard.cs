// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;

namespace Cratis.Specifications.Runner;

/// <summary>
/// Represents one shard of a run split across several machines.
/// </summary>
/// <param name="Index">The 1-based index of the shard.</param>
/// <param name="Count">The number of shards.</param>
internal readonly record struct Shard(int Index, int Count)
{
    /// <summary>
    /// Parses a shard in the form "index/count", for instance "2/4".
    /// </summary>
    /// <param name="value">The value to parse.</param>
    /// <param name="shard">The parsed shard.</param>
    /// <returns>True if the value is a valid shard; otherwise false.</returns>
    public static bool TryParse(string value, out Shard shard)
    {
        shard = default;
        var parts = value.Split('/');
        if (parts.Length != 2 ||
            !int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var index) ||
            !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var count) ||
            count < 1 || index < 1 || index > count)
        {
            return false;
        }

        shard = new(index, count);
        return true;
    }
}
