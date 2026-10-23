// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;

namespace Cratis.Specifications;

/// <summary>
/// Compares, counts and formats the values the "Should*" assertions work on, with the same rules in every package.
/// </summary>
/// <remarks>
/// Strings compare by value. Any other pair of collections compares item by item, recursively, so nested collections are
/// compared deeply. Everything else uses <see cref="EqualityComparer{T}.Default"/>; <c>null</c> equals only <c>null</c>.
/// Comparing and counting avoid LINQ and closures, because they run on every passing assertion; formatting only runs on failure.
/// </remarks>
static class AssertionValues
{
    public static bool AreEqual<T>(T expected, T actual)
    {
        if (expected is string || actual is string) return Equals(expected, actual);
        if (expected is IEnumerable expectedItems && actual is IEnumerable actualItems) return SequencesEqual(expectedItems, actualItems);
        return EqualityComparer<T>.Default.Equals(expected, actual);
    }

    public static bool Contains<T>(IEnumerable<T> collection, T expected)
    {
        foreach (var item in collection)
        {
            if (AreEqual(expected, item)) return true;
        }

        return false;
    }

    public static bool ContainsMatch<T>(IEnumerable<T> collection, Func<T, bool> filter)
    {
        foreach (var item in collection)
        {
            if (filter(item)) return true;
        }

        return false;
    }

    public static int IndexOf<T>(List<T> items, T expected)
    {
        for (var index = 0; index < items.Count; index++)
        {
            if (AreEqual(expected, items[index])) return index;
        }

        return -1;
    }

    public static int CountUpTo(IEnumerable collection, int limit)
    {
        if (collection is ICollection items) return items.Count;
        var enumerator = collection.GetEnumerator();
        using var disposable = enumerator as IDisposable;
        var count = 0;
        while (count < limit && enumerator.MoveNext()) count++;
        return count;
    }

    public static int Count(IEnumerable collection) => CountUpTo(collection, int.MaxValue);

    public static string Format(object? value) => value switch
    {
        null => "null",
        string text => $"\"{text}\"",
        IEnumerable items => "[" + string.Join(", ", items.Cast<object?>().Select(Format)) + "]",
        _ => value.ToString() ?? value.GetType().ToString()
    };

    static bool SequencesEqual(IEnumerable expected, IEnumerable actual)
    {
        var expectedEnumerator = expected.GetEnumerator();
        using var expectedDisposable = expectedEnumerator as IDisposable;
        var actualEnumerator = actual.GetEnumerator();
        using var actualDisposable = actualEnumerator as IDisposable;
        while (true)
        {
            var expectedHasNext = expectedEnumerator.MoveNext();
            var actualHasNext = actualEnumerator.MoveNext();
            if (expectedHasNext != actualHasNext) return false;
            if (!expectedHasNext) return true;
            if (!AreEqual<object?>(expectedEnumerator.Current, actualEnumerator.Current)) return false;
        }
    }
}
