// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Cratis.Specifications;

/// <summary>
/// Compares, counts and formats the values the "Should*" assertions work on, with the same rules in every package.
/// </summary>
/// <remarks>
/// Null equals only null; strings compare by value. Self-equatable types take precedence over collections, as in xUnit 2.9.3.
/// Statically typed IEquatable values use EqualityComparer.Default; runtime-typed values use Object.Equals, relying on
/// the implementor to forward that overload to its equatable equality. Dictionaries compare as unordered key/value maps,
/// sets as unordered sets (xUnit's rule), and other enumerable pairs in order, recursively. Only then does ordinary
/// equality, including overridden Object.Equals, apply. Thus an enumerable with only an Equals override stays ordered.
/// Comparing and counting avoid LINQ and closures, because they run on every passing assertion; formatting only runs on failure.
/// </remarks>
static class AssertionValues
{
    static readonly ConcurrentDictionary<Type, bool> _equatableTypes = new();
    static readonly ConcurrentDictionary<Type, Type[]> _interfaces = new();
    static readonly ConcurrentDictionary<Type, (PropertyInfo? Key, PropertyInfo? Value)> _entryProperties = new();

    public static bool AreEqual<T>(T expected, T actual)
    {
        if (expected is null) return actual is null;
        if (actual is null) return false;
        if (expected is string || actual is string) return Equals(expected, actual);
        if (Equality<T>.IsEquatable) return EqualityComparer<T>.Default.Equals(expected, actual);
        if (expected is IEnumerable expectedItems && actual is IEnumerable actualItems) return CollectionsEqual(expectedItems, actualItems);
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

    /// <summary>
    /// Extracts the expected and actual values from a message in the "Expected: ... / Actual: ..." shape the assertions format.
    /// </summary>
    /// <param name="message">The failure message.</param>
    /// <param name="expected">The expected value, without its label.</param>
    /// <param name="actual">The actual value, without its label.</param>
    /// <returns>True when the message has both values.</returns>
    public static bool TryExtractExpectedActual(string message, out string? expected, out string? actual)
    {
        expected = null;
        actual = null;
        if (!message.StartsWith("Expected", StringComparison.Ordinal)) return false;
        var separator = message.IndexOf(Environment.NewLine + "Actual:", StringComparison.Ordinal);
        if (separator < 0) return false;

        var expectedText = message.Substring(0, separator);
        expected = expectedText.StartsWith("Expected: ", StringComparison.Ordinal) ? expectedText.Substring("Expected: ".Length) : expectedText;
        actual = message.Substring(separator + Environment.NewLine.Length + "Actual:".Length).TrimStart();
        return true;
    }

    static bool CollectionsEqual(IEnumerable expected, IEnumerable actual)
    {
        if (_equatableTypes.GetOrAdd(expected.GetType(), IsSelfEquatable)) return expected.Equals(actual);
        if (IsDictionary(expected) && IsDictionary(actual)) return UnorderedEqual(expected, actual, UnorderedComparison.Entries);
        if (IsSet(expected) && IsSet(actual)) return UnorderedEqual(expected, actual, UnorderedComparison.Items);
        return SequencesEqual(expected, actual);
    }

    static bool IsSelfEquatable(Type type)
    {
        foreach (var contract in Interfaces(type))
        {
            if (!contract.IsGenericType) continue;
            if (contract.GetGenericTypeDefinition() != typeof(IEquatable<>)) continue;
            if (contract.GenericTypeArguments[0] == type) return true;
        }

        return false;
    }

    static bool IsDictionary(IEnumerable value) => value is IDictionary || Implements(value.GetType(), typeof(IDictionary<,>), typeof(IReadOnlyDictionary<,>));

    static bool IsSet(IEnumerable value) => Implements(value.GetType(), typeof(ISet<>), typeof(IReadOnlySet<>));

    static bool Implements(Type type, Type first, Type second)
    {
        foreach (var contract in Interfaces(type))
        {
            if (!contract.IsGenericType) continue;
            var definition = contract.GetGenericTypeDefinition();
            if (definition == first || definition == second) return true;
        }

        return false;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Only interfaces implemented by constructed runtime types are inspected; their implementations are preserved.")]
    static Type[] Interfaces(Type type) => _interfaces.GetOrAdd(type, static type => type.GetInterfaces());

    static bool UnorderedEqual(IEnumerable expected, IEnumerable actual, UnorderedComparison comparison)
    {
        var remaining = new List<object?>();
        foreach (var item in actual) remaining.Add(item);
        foreach (var item in expected)
        {
            var index = FindUnorderedItem(remaining, item, comparison);
            if (index < 0) return false;
            remaining.RemoveAt(index);
        }

        return remaining.Count == 0;
    }

    static int FindUnorderedItem(List<object?> items, object? expected, UnorderedComparison comparison)
    {
        for (var index = 0; index < items.Count; index++)
        {
            if (comparison == UnorderedComparison.Entries ? EntriesEqual(expected!, items[index]!) : AreEqual(expected, items[index])) return index;
        }

        return -1;
    }

    static bool EntriesEqual(object expected, object actual)
    {
        // Without reflection metadata for the entry type (possible in native AOT), the entries compare with their own equality.
        if (!TryEntry(expected, out var expectedEntry) || !TryEntry(actual, out var actualEntry)) return Equals(expected, actual);
        return AreEqual(expectedEntry.Key, actualEntry.Key) && AreEqual(expectedEntry.Value, actualEntry.Value);
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(KeyValuePair<,>))]
    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Generic dictionary enumerators yield KeyValuePair instances, whose public properties are explicitly preserved.")]
    static bool TryEntry(object item, out (object? Key, object? Value) entry)
    {
        if (item is DictionaryEntry dictionaryEntry)
        {
            entry = (dictionaryEntry.Key, dictionaryEntry.Value);
            return true;
        }

        var properties = _entryProperties.GetOrAdd(item.GetType(), static type => (type.GetProperty("Key"), type.GetProperty("Value")));
        if (properties.Key is null || properties.Value is null)
        {
            entry = default;
            return false;
        }

        entry = (properties.Key.GetValue(item), properties.Value.GetValue(item));
        return true;
    }

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

    enum UnorderedComparison
    {
        Items,
        Entries
    }

    static class Equality<T>
    {
        public static readonly bool IsEquatable = typeof(IEquatable<T>).IsAssignableFrom(typeof(T));
    }
}
