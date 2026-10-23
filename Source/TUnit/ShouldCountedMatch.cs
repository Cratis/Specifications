// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications;

/// <summary>
/// Decides in linear time whether two collections hold the same items regardless of order, for the successful path of ShouldContainOnly.
/// </summary>
/// <remarks>
/// It only ever answers "yes, they match" or "not sure": a negative answer makes the caller run the original
/// remove-one-by-one algorithm, which also produces the failure message, so failures and edge cases keep their exact behavior.
/// </remarks>
static class ShouldCountedMatch
{
    /// <summary>
    /// Below this many items the quadratic algorithm is cheaper than building a lookup.
    /// </summary>
    internal const int Threshold = 16;

    /// <summary>
    /// Gets whether every item occurs equally often in both collections, using the default equality comparer (as <see cref="List{T}.Remove"/> does).
    /// </summary>
    /// <typeparam name="T">Type of element.</typeparam>
    /// <param name="source">The actual items.</param>
    /// <param name="expected">The expected items.</param>
    /// <returns>True when the collections certainly match; false when they differ or the lookup cannot tell.</returns>
    internal static bool Matches<T>(List<T> source, ICollection<T> expected)
    {
        if (source.Count < Threshold || source.Count != expected.Count)
        {
            return false;
        }

        // Nulls cannot be dictionary keys; they are counted on the side.
        var nulls = 0;
#pragma warning disable CS8714 // Null items are never used as keys.
        var counts = new Dictionary<T, int>(source.Count);
#pragma warning restore CS8714
        foreach (var item in source)
        {
            if (item is null)
            {
                nulls++;
            }
            else
            {
                counts[item] = counts.TryGetValue(item, out var count) ? count + 1 : 1;
            }
        }

        foreach (var item in expected)
        {
            if (item is null)
            {
                if (nulls-- == 0) return false;
            }
            else if (counts.TryGetValue(item, out var count) && count > 0)
            {
                counts[item] = count - 1;
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Gets the items of an enumerable as a collection, enumerating it exactly once.
    /// </summary>
    /// <typeparam name="T">Type of element.</typeparam>
    /// <param name="items">The items.</param>
    /// <returns>The items as a collection.</returns>
    internal static ICollection<T> AsCollection<T>(IEnumerable<T> items)
    {
        if (items is ICollection<T> collection)
        {
            return collection;
        }

        var list = new List<T>();
        foreach (var item in items)
        {
            list.Add(item);
        }

        return list;
    }
}
