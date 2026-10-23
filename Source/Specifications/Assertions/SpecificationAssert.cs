// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Cratis.Specifications;

/// <summary>
/// Framework-neutral assertions backing the "Should*" extension methods.
/// </summary>
/// <remarks>
/// TUnit assertions are asynchronous, which does not fit the synchronous, one-line "should_" facts.
/// These assertions are therefore self-contained and throw <see cref="SpecificationAssertionException"/>.
/// </remarks>
static class SpecificationAssert
{
    public static void Fail(string message) => throw new SpecificationAssertionException(message);

    [DoesNotReturn]
    public static void FailActualWasNull(string assertion, string expectation) =>
        throw new SpecificationAssertionException($"{assertion} expected {expectation} but the actual value was null");

    public static void True(bool condition, string? message = null)
    {
        if (!condition) Fail(message ?? $"Expected: True{Environment.NewLine}Actual:   False");
    }

    public static void False(bool condition)
    {
        if (condition) Fail("Expected: False" + Environment.NewLine + "Actual:   True");
    }

    public static void Null(object? actual)
    {
        if (actual is not null) Fail($"Expected: null{Environment.NewLine}Actual:   {Format(actual)}");
    }

    public static void NotNull(object? actual)
    {
        if (actual is null) Fail("Expected: not null" + Environment.NewLine + "Actual:   null");
    }

    public static void Equal<T>(T expected, T actual)
    {
        if (!AreEqual(expected, actual)) Fail($"Expected: {Format(expected)}{Environment.NewLine}Actual:   {Format(actual)}");
    }

    public static void NotEqual<T>(T expected, T actual)
    {
        if (AreEqual(expected, actual)) Fail($"Expected: not {Format(expected)}{Environment.NewLine}Actual:   {Format(actual)}");
    }

    public static void NotStrictEqual<T>(T expected, T actual) => NotEqual(expected, actual);

    public static void Same(object? expected, object? actual)
    {
        if (!ReferenceEquals(expected, actual)) Fail($"Expected the same instance as: {Format(expected)}{Environment.NewLine}Actual: {Format(actual)}");
    }

    public static void NotSame(object? expected, object? actual)
    {
        if (ReferenceEquals(expected, actual)) Fail($"Expected a different instance than: {Format(expected)}");
    }

    public static void Contains<T>(T expected, IEnumerable<T> collection)
    {
        if (!collection.Any(item => AreEqual(expected, item))) Fail($"Collection does not contain {Format(expected)}{Environment.NewLine}Collection: {Format(collection)}");
    }

    public static void Contains<T>(IEnumerable<T> collection, Predicate<T> filter)
    {
        if (!collection.Any(item => filter(item))) Fail($"Collection does not contain any item matching the filter{Environment.NewLine}Collection: {Format(collection)}");
    }

    public static void Contains<TKey, TValue>(TKey expected, IDictionary<TKey, TValue> actual)
        where TKey : notnull
    {
        if (!actual.ContainsKey(expected)) Fail($"Dictionary does not contain key {Format(expected)}{Environment.NewLine}Keys: {Format(actual.Keys)}");
    }

    public static void Contains(string expectedSubstring, string? actual, StringComparison comparisonType)
    {
        if (actual is null || !actual.Contains(expectedSubstring, comparisonType)) Fail($"String does not contain {Format(expectedSubstring)}{Environment.NewLine}String: {Format(actual)}");
    }

    public static void DoesNotContain<T>(T expected, IEnumerable<T> collection)
    {
        if (collection.Any(item => AreEqual(expected, item))) Fail($"Collection contains {Format(expected)}{Environment.NewLine}Collection: {Format(collection)}");
    }

    public static void DoesNotContain<T>(IEnumerable<T> collection, Predicate<T> filter)
    {
        if (collection.Any(item => filter(item))) Fail($"Collection contains an item matching the filter{Environment.NewLine}Collection: {Format(collection)}");
    }

    public static void DoesNotContain<TKey, TValue>(TKey expected, IDictionary<TKey, TValue> actual)
        where TKey : notnull
    {
        if (actual.ContainsKey(expected)) Fail($"Dictionary contains key {Format(expected)}");
    }

    public static void DoesNotContain(string expectedSubstring, string? actual, StringComparison comparisonType)
    {
        if (actual is null) FailActualWasNull("ShouldNotContain", "the actual value not to contain the specified item");
        if (actual.Contains(expectedSubstring, comparisonType)) Fail($"String contains {Format(expectedSubstring)}{Environment.NewLine}String: {Format(actual)}");
    }

    public static void Empty(IEnumerable collection)
    {
        if (collection.Cast<object?>().Any()) Fail($"Expected an empty collection{Environment.NewLine}Actual:   {Format(collection)}");
    }

    public static void NotEmpty(IEnumerable collection)
    {
        if (!collection.Cast<object?>().Any()) Fail("Expected a non-empty collection" + Environment.NewLine + "Actual:   []");
    }

    public static void Single(IEnumerable collection)
    {
        var count = collection.Cast<object?>().Count();
        if (count != 1) Fail($"Expected a collection with exactly one item{Environment.NewLine}Actual:   {count} items: {Format(collection)}");
    }

    public static void InRange<T>(T actual, T low, T high)
        where T : IComparable
    {
        if (!IsInRange(actual, low, high)) Fail($"Expected: in range ({Format(low)} - {Format(high)}){Environment.NewLine}Actual:   {Format(actual)}");
    }

    public static void NotInRange<T>(T actual, T low, T high)
        where T : IComparable
    {
        if (IsInRange(actual, low, high)) Fail($"Expected: not in range ({Format(low)} - {Format(high)}){Environment.NewLine}Actual:   {Format(actual)}");
    }

    public static void IsAssignableFrom<T>(object? actual) => IsAssignableFrom(typeof(T), actual);

    public static void IsAssignableFrom(Type expected, object? actual)
    {
        if (actual is null || !expected.IsInstanceOfType(actual)) Fail($"Expected: an instance assignable to {expected}{Environment.NewLine}Actual:   {actual?.GetType().ToString() ?? "null"}");
    }

    public static void IsType<T>(object? actual)
    {
        if (actual is null || actual.GetType() != typeof(T)) Fail($"Expected: {typeof(T)}{Environment.NewLine}Actual:   {actual?.GetType().ToString() ?? "null"}");
    }

    static bool IsInRange<T>(T actual, T low, T high)
        where T : IComparable =>
        Comparer<T>.Default.Compare(actual, low) >= 0 && Comparer<T>.Default.Compare(actual, high) <= 0;

    static bool AreEqual<T>(T expected, T actual)
    {
        if (expected is string || actual is string) return Equals(expected, actual);
        if (expected is IEnumerable expectedItems && actual is IEnumerable actualItems) return SequencesEqual(expectedItems, actualItems);
        return EqualityComparer<T>.Default.Equals(expected, actual);
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

    static string Format(object? value) => value switch
    {
        null => "null",
        string text => $"\"{text}\"",
        IEnumerable items => "[" + string.Join(", ", items.Cast<object?>().Select(Format)) + "]",
        _ => value.ToString() ?? value.GetType().ToString()
    };
}
