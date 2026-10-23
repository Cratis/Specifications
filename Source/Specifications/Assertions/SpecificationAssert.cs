// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using static Cratis.Specifications.AssertionValues;

namespace Cratis.Specifications;

/// <summary>
/// Framework-neutral assertions backing the "Should*" extension methods.
/// </summary>
/// <remarks>
/// Every package uses these, so the rules and the messages are the same everywhere. A failure is thrown as the exception
/// <see cref="AssertionFailures"/> creates for the test framework in use. Messages are only built once a check has failed.
/// </remarks>
static class SpecificationAssert
{
    [DoesNotReturn]
    public static void Fail(string message) => throw AssertionFailures.Create(message);

    [DoesNotReturn]
    public static void FailActualWasNull(string assertion, string expectation) =>
        Fail($"{assertion} expected {expectation} but the actual value was null");

    public static void True(bool condition)
    {
        if (!condition) Fail($"Expected: True{Environment.NewLine}Actual:   False");
    }

    public static void False(bool condition)
    {
        if (condition) Fail("Expected: False" + Environment.NewLine + "Actual:   True");
    }

    public static void Null(object? actual)
    {
        if (actual is not null) Fail($"Expected: null{Environment.NewLine}Actual:   {Format(actual)}");
    }

    public static void NotNull([NotNull] object? actual)
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

    public static void Same(object? expected, object? actual)
    {
        if (!ReferenceEquals(expected, actual)) Fail($"Expected the same instance as: {Format(expected)}{Environment.NewLine}Actual: {Format(actual)}");
    }

    public static void NotSame(object? expected, object? actual)
    {
        if (ReferenceEquals(expected, actual)) Fail($"Expected a different instance than: {Format(expected)}");
    }

    public static void ContainsOnly<T>(IEnumerable<T>? expected, IEnumerable<T> collection)
    {
        if (expected is null) Fail($"Expected: null{Environment.NewLine}Actual:   {Format(collection)}");
        var remaining = new List<T>(collection);
        var allFound = true;
        foreach (var item in expected)
        {
            var index = IndexOf(remaining, item);
            if (index < 0)
            {
                allFound = false;
                break;
            }

            remaining.RemoveAt(index);
        }

        if (!allFound || remaining.Count > 0) Fail($"Collection '{string.Join(",", collection)}' does not only contain '{string.Join(",", expected)}'");
    }

    public static void Contains<T>(T expected, IEnumerable<T> collection)
    {
        if (!AssertionValues.Contains(collection, expected)) Fail($"Collection does not contain {Format(expected)}{Environment.NewLine}Collection: {Format(collection)}");
    }

    public static void Contains<T>(IEnumerable<T> collection, Func<T, bool> filter)
    {
        if (!ContainsMatch(collection, filter)) Fail($"Collection does not contain any item matching the filter{Environment.NewLine}Collection: {Format(collection)}");
    }

    public static void Contains<TKey, TValue>(TKey expected, IDictionary<TKey, TValue> actual)
        where TKey : notnull
    {
        if (!actual.ContainsKey(expected)) Fail($"Dictionary does not contain key {Format(expected)}{Environment.NewLine}Keys: {Format(actual.Keys)}");
    }

    public static void Contains(string expectedSubstring, string? actual, StringComparison comparisonType)
    {
        if (actual is null) FailActualWasNull("ShouldContain", "the actual value to contain the expected item");
        if (!actual.Contains(expectedSubstring, comparisonType)) Fail($"String does not contain {Format(expectedSubstring)}{Environment.NewLine}String: {Format(actual)}");
    }

    public static void DoesNotContain<T>(T expected, IEnumerable<T> collection)
    {
        if (AssertionValues.Contains(collection, expected)) Fail($"Collection contains {Format(expected)}{Environment.NewLine}Collection: {Format(collection)}");
    }

    public static void DoesNotContain<T>(IEnumerable<T> collection, Func<T, bool> filter)
    {
        if (ContainsMatch(collection, filter)) Fail($"Collection contains an item matching the filter{Environment.NewLine}Collection: {Format(collection)}");
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
        if (CountUpTo(collection, 1) != 0) Fail($"Expected an empty collection{Environment.NewLine}Actual:   {Format(collection)}");
    }

    public static void NotEmpty(IEnumerable collection)
    {
        if (CountUpTo(collection, 1) == 0) Fail("Expected a non-empty collection" + Environment.NewLine + "Actual:   []");
    }

    public static void Single(IEnumerable collection)
    {
        if (CountUpTo(collection, 2) != 1) Fail($"Expected a collection with exactly one item{Environment.NewLine}Actual:   {Count(collection)} items: {Format(collection)}");
    }

    public static void Compared(bool holds, IComparable left, string relation, IComparable? right)
    {
        if (!holds) Fail($"{left} should be {relation} {right}");
    }

    public static void InRange<T>(T actual, T low, T high)
        where T : IComparable
    {
        if (actual is null) FailActualWasNull("ShouldBeInRange", $"a value in range ({Format(low)} - {Format(high)})");
        if (!IsInRange(actual, low, high)) Fail($"Expected: in range ({Format(low)} - {Format(high)}){Environment.NewLine}Actual:   {Format(actual)}");
    }

    public static void NotInRange<T>(T actual, T low, T high)
        where T : IComparable
    {
        if (actual is null) FailActualWasNull("ShouldNotBeInRange", $"a value not in range ({Format(low)} - {Format(high)})");
        if (IsInRange(actual, low, high)) Fail($"Expected: not in range ({Format(low)} - {Format(high)}){Environment.NewLine}Actual:   {Format(actual)}");
    }

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
}
