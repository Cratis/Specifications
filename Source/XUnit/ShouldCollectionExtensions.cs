// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using Xunit;

// Failures keep raising the TrueException (and message) of the former Assert.True(condition, message) calls;
// the message is only built once the condition is known to have failed.
#pragma warning disable xUnit2020

namespace Cratis.Specifications;

/// <summary>
/// Holds extension methods for fluent "Should*" assertions related to collections.
/// </summary>
public static class ShouldCollectionExtensions
{
    /// <summary>
    /// Assert that a collection only contains the expected elements.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    /// <param name="expected">Expected values.</param>
    /// <typeparam name="T">Type of element.</typeparam>
    public static void ShouldContainOnly<T>(this IEnumerable<T> collection, IEnumerable<T> expected)
    {
        if (collection is null) NullActual.Fail("ShouldContainOnly", "a collection containing only the expected items");
        var source = new List<T>(collection);
        var noContain = new List<T>();

        foreach (var item in expected)
        {
            if (!source.Remove(item))
            {
                noContain.Add(item);
            }
        }

        if (noContain.Count > 0 || source.Count > 0)
        {
            var sourceItems = string.Join(",", collection);
            var expectedItems = string.Join(",", expected);

            Assert.Fail($"Collection '{sourceItems}' does not only contain '{expectedItems}'");
        }
    }

    /// <summary>
    /// Assert that a collection only contains the expected elements - based on params.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    /// <param name="expected">Expected values.</param>
    /// <typeparam name="T">Type of element.</typeparam>
    public static void ShouldContainOnly<T>(this IEnumerable<T> collection, params T[] expected)
    {
        collection.ShouldContainOnly(expected as IEnumerable<T>);
    }

    /// <summary>
    /// Assert that a collection contains exactly only the expected elements in the same sequence.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    /// <param name="expected">Expected values.</param>
    /// <typeparam name="T">Type of element.</typeparam>
    public static void ShouldEqual<T>(this IEnumerable<T> collection, IEnumerable<T> expected)
    {
        if (collection is null) NullActual.Fail("ShouldEqual", "a collection equal to the expected items");
        if (collection.SequenceEqual(expected)) return;

        Assert.True(false, $"Collection '{collection}' is not equal to '{expected}'");
    }

    /// <summary>
    /// Assert that a collection contains exactly only the expected elements in the same sequence - based on params.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    /// <param name="expected">Expected values.</param>
    /// <typeparam name="T">Type of element.</typeparam>
    public static void ShouldEqual<T>(this IEnumerable<T> collection, params T[] expected)
    {
        collection.ShouldEqual(expected as IEnumerable<T>);
    }

    /// <summary>
    /// Assert that a collection contains all the expected elements.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    /// <param name="expected">Expected elements.</param>
    /// <typeparam name="T">Type of element.</typeparam>
    public static void ShouldContain<T>(this IEnumerable<T> collection, IEnumerable<T> expected)
    {
        if (collection is null) NullActual.Fail("ShouldContain", "the actual value to contain the expected item");
        foreach (var item in expected)
        {
            Assert.Contains(item, collection);
        }
    }

    /// <summary>
    /// Assert that a collection contains all the expected elements - based on params.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    /// <param name="expected">Expected elements as params.</param>
    /// <typeparam name="T">Type of element.</typeparam>
    public static void ShouldContain<T>(this IEnumerable<T> collection, params T[] expected)
    {
        collection.ShouldContain(expected as IEnumerable<T>);
    }

    /// <summary>
    /// Assert that a dictionary contains a specific key.
    /// </summary>
    /// <param name="actual">Dictionary to assert.</param>
    /// <param name="expected">Expected key.</param>
    /// <typeparam name="TKey">Type of key.</typeparam>
    /// <typeparam name="TValue">Type of value.</typeparam>
    public static void ShouldContain<TKey, TValue>(this IDictionary<TKey, TValue> actual, TKey expected)
        where TKey : notnull
    {
        if (actual is null) NullActual.Fail("ShouldContain", "the actual value to contain the expected item");
        Assert.Contains(expected, actual);
    }

    /// <summary>
    /// Assert that a collection contains specific element(s) based on a predicate filter.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    /// <param name="filter">Filter to apply.</param>
    /// <typeparam name="T">Type of element.</typeparam>
    public static void ShouldContain<T>(this IEnumerable<T> collection, Predicate<T> filter)
    {
        if (collection is null) NullActual.Fail("ShouldContain", "the actual value to contain the expected item");
        Assert.Contains(collection, filter);
    }

    /// <summary>
    /// Assert that a collection contains a specific element.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    /// <param name="expected">Expected element.</param>
    /// <typeparam name="T">Type of element.</typeparam>
    public static void ShouldContain<T>(this IEnumerable<T> collection, T expected)
    {
        if (collection is null) NullActual.Fail("ShouldContain", "the actual value to contain the expected item");
        Assert.Contains(expected, collection);
    }

    /// <summary>
    /// Assert that a dictionary does not contain a specific key.
    /// </summary>
    /// <param name="actual">Dictionary to assert.</param>
    /// <param name="expected">Not expected key.</param>
    /// <typeparam name="TKey">Type of key.</typeparam>
    /// <typeparam name="TValue">Type of value.</typeparam>
    public static void ShouldNotContain<TKey, TValue>(this IDictionary<TKey, TValue> actual, TKey expected)
        where TKey : notnull
    {
        if (actual is null) NullActual.Fail("ShouldNotContain", "the actual value not to contain the specified item");
        Assert.DoesNotContain(expected, actual);
    }

    /// <summary>
    /// Assert that a collection does not contain specific element(s) based on a predicate filter.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    /// <param name="filter">Filter to apply.</param>
    /// <typeparam name="T">Type of element.</typeparam>
    public static void ShouldNotContain<T>(this IEnumerable<T> collection, Predicate<T> filter)
    {
        if (collection is null) NullActual.Fail("ShouldNotContain", "the actual value not to contain the specified item");
        Assert.DoesNotContain(collection, filter);
    }

    /// <summary>
    /// Assert that all items in a collection are conforming based on the decision of a callback.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    /// <param name="expected">Callback that will check conformity.</param>
    /// <typeparam name="T">Type of element.</typeparam>
    public static void ShouldEachConformTo<T>(this IEnumerable<T> collection, Func<T, bool> expected)
    {
        if (collection is null) NullActual.Fail("ShouldEachConformTo", "every item to conform to the filter");
        foreach (var item in collection)
        {
            item.ShouldMatch(expected);
        }
    }

    /// <summary>
    /// Assert that a collection does not contain a specific element.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    /// <param name="expected">Expected element.</param>
    /// <typeparam name="T">Type of element.</typeparam>
    public static void ShouldNotContain<T>(this IEnumerable<T> collection, T expected)
    {
        if (collection is null) NullActual.Fail("ShouldNotContain", "the actual value not to contain the specified item");
        Assert.DoesNotContain(expected, collection);
    }

    /// <summary>
    /// Assert that a collection is empty.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    public static void ShouldBeEmpty(this IEnumerable collection)
    {
        if (collection is null) NullActual.Fail("ShouldBeEmpty", "an empty collection");
        Assert.Empty(collection);
    }

    /// <summary>
    /// Assert that a collection is not empty.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    public static void ShouldNotBeEmpty(this IEnumerable collection)
    {
        if (collection is null) NullActual.Fail("ShouldNotBeEmpty", "a non-empty collection");
        Assert.NotEmpty(collection);
    }

    /// <summary>
    /// Assert that a collection has a single item.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    public static void ShouldContainSingleItem(this IEnumerable collection)
    {
        if (collection is null) NullActual.Fail("ShouldContainSingleItem", "a collection with a single item");
        Assert.Single(collection);
    }

    /// <summary>
    /// Assert that a collection contains exactly the expected elements in the same sequence. Explicit, framework-independent alternative to <c>ShouldEqual</c>.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    /// <param name="expected">Expected values.</param>
    /// <typeparam name="T">Type of element.</typeparam>
    public static void ShouldEqualInOrder<T>(this IEnumerable<T> collection, IEnumerable<T> expected)
    {
        if (collection is null) NullActual.Fail("ShouldEqualInOrder", "a collection equal to the expected items");
        if (expected is null) Assert.Fail("ShouldEqualInOrder expected a collection equal to the expected items but the expected value was null");
        if (collection.SequenceEqual(expected)) return;

        Assert.True(false, $"Collection '{collection}' is not equal to '{expected}'");
    }

    /// <summary>
    /// Assert that a collection contains exactly the expected elements in the same sequence - based on params.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    /// <param name="expected">Expected values.</param>
    /// <typeparam name="T">Type of element.</typeparam>
    public static void ShouldEqualInOrder<T>(this IEnumerable<T> collection, params T[] expected)
    {
        collection.ShouldEqualInOrder(expected as IEnumerable<T>);
    }

    /// <summary>
    /// Assert that a collection contains exactly the expected elements, in any order. Duplicates are significant, so every expected element must occur as many times as expected.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    /// <param name="expected">Expected values.</param>
    /// <typeparam name="T">Type of element.</typeparam>
    public static void ShouldEqualInAnyOrder<T>(this IEnumerable<T> collection, IEnumerable<T> expected)
    {
        collection.ShouldContainOnly(expected);
    }

    /// <summary>
    /// Assert that a collection contains exactly the expected elements, in any order - based on params.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    /// <param name="expected">Expected values.</param>
    /// <typeparam name="T">Type of element.</typeparam>
    public static void ShouldEqualInAnyOrder<T>(this IEnumerable<T> collection, params T[] expected)
    {
        collection.ShouldEqualInAnyOrder(expected as IEnumerable<T>);
    }

    /// <summary>
    /// Assert that a collection contains ONE specific element based on a predicate filter.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    /// <param name="filter">Filter to apply.</param>
    /// <typeparam name="T">Type of element.</typeparam>
    public static void ShouldContainSingleMatching<T>(this IEnumerable<T> collection, Predicate<T> filter)
    {
        if (collection is null) NullActual.Fail("ShouldContainSingleMatching", "exactly one item matching the filter");
        var matches = 0;
        foreach (var item in collection)
        {
            if (filter(item)) matches++;
        }

        if (matches != 1)
        {
            Assert.Fail($"Expected exactly one item matching the filter but found {matches}");
        }
    }
}
