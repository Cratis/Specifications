// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;

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
        if (collection is null) SpecificationAssert.FailActualWasNull("ShouldContainOnly", "a collection containing only the expected items");
        var source = new List<T>(collection);
        var expectedItems = ShouldCountedMatch.AsCollection(expected);
        if (ShouldCountedMatch.Matches(source, expectedItems)) return;

        var noContain = new List<T>();

        foreach (var item in expectedItems)
        {
            if (!source.Remove(item))
            {
                noContain.Add(item);
            }
        }

        if (noContain.Count > 0 || source.Count > 0)
        {
            var sourceItems = string.Join(",", collection);
            var expectedText = string.Join(",", expected);

            SpecificationAssert.Fail($"Collection '{sourceItems}' does not only contain '{expectedText}'");
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
        SpecificationAssert.Equal(expected, collection);
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
        if (collection is null) SpecificationAssert.FailActualWasNull("ShouldContain", "the actual value to contain the expected item");
        foreach (var item in expected)
        {
            SpecificationAssert.Contains(item, collection);
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
        if (actual is null) SpecificationAssert.FailActualWasNull("ShouldContain", "the actual value to contain the expected item");
        SpecificationAssert.Contains(expected, actual);
    }

    /// <summary>
    /// Assert that a collection contains specific element(s) based on a predicate filter.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    /// <param name="filter">Filter to apply.</param>
    /// <typeparam name="T">Type of element.</typeparam>
    public static void ShouldContain<T>(this IEnumerable<T> collection, Predicate<T> filter)
    {
        if (collection is null) SpecificationAssert.FailActualWasNull("ShouldContain", "the actual value to contain the expected item");
        SpecificationAssert.Contains(collection, filter);
    }

    /// <summary>
    /// Assert that a collection contains a specific element.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    /// <param name="expected">Expected element.</param>
    /// <typeparam name="T">Type of element.</typeparam>
    public static void ShouldContain<T>(this IEnumerable<T> collection, T expected)
    {
        if (collection is null) SpecificationAssert.FailActualWasNull("ShouldContain", "the actual value to contain the expected item");
        SpecificationAssert.Contains(expected, collection);
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
        if (actual is null) SpecificationAssert.FailActualWasNull("ShouldNotContain", "the actual value not to contain the specified item");
        SpecificationAssert.DoesNotContain(expected, actual);
    }

    /// <summary>
    /// Assert that a collection does not contain specific element(s) based on a predicate filter.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    /// <param name="filter">Filter to apply.</param>
    /// <typeparam name="T">Type of element.</typeparam>
    public static void ShouldNotContain<T>(this IEnumerable<T> collection, Predicate<T> filter)
    {
        if (collection is null) SpecificationAssert.FailActualWasNull("ShouldNotContain", "the actual value not to contain the specified item");
        SpecificationAssert.DoesNotContain(collection, filter);
    }

    /// <summary>
    /// Assert that all items in a collection are conforming based on the decision of a callback.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    /// <param name="expected">Callback that will check conformity.</param>
    /// <typeparam name="T">Type of element.</typeparam>
    public static void ShouldEachConformTo<T>(this IEnumerable<T> collection, Func<T, bool> expected)
    {
        if (collection is null) SpecificationAssert.FailActualWasNull("ShouldEachConformTo", "every item to conform to the filter");
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
        if (collection is null) SpecificationAssert.FailActualWasNull("ShouldNotContain", "the actual value not to contain the specified item");
        SpecificationAssert.DoesNotContain(expected, collection);
    }

    /// <summary>
    /// Assert that a collection is empty.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    public static void ShouldBeEmpty(this IEnumerable collection)
    {
        if (collection is null) SpecificationAssert.FailActualWasNull("ShouldBeEmpty", "an empty collection");
        SpecificationAssert.Empty(collection);
    }

    /// <summary>
    /// Assert that a collection is not empty.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    public static void ShouldNotBeEmpty(this IEnumerable collection)
    {
        if (collection is null) SpecificationAssert.FailActualWasNull("ShouldNotBeEmpty", "a non-empty collection");
        SpecificationAssert.NotEmpty(collection);
    }

    /// <summary>
    /// Assert that a collection has a single item.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    public static void ShouldContainSingleItem(this IEnumerable collection)
    {
        if (collection is null) SpecificationAssert.FailActualWasNull("ShouldContainSingleItem", "a collection with a single item");
        SpecificationAssert.Single(collection);
    }

    /// <summary>
    /// Assert that a collection contains exactly the expected elements in the same sequence. Explicit, framework-independent alternative to <c>ShouldEqual</c>.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    /// <param name="expected">Expected values.</param>
    /// <typeparam name="T">Type of element.</typeparam>
    public static void ShouldEqualInOrder<T>(this IEnumerable<T> collection, IEnumerable<T> expected)
    {
        if (collection is null) SpecificationAssert.FailActualWasNull("ShouldEqualInOrder", "a collection equal to the expected items");
        SpecificationAssert.Equal(expected, collection);
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
        if (collection is null) SpecificationAssert.FailActualWasNull("ShouldContainSingleMatching", "exactly one item matching the filter");
        var matches = 0;
        foreach (var item in collection)
        {
            if (filter(item)) matches++;
        }

        if (matches != 1)
        {
            SpecificationAssert.Fail($"Expected exactly one item matching the filter but found {matches}");
        }
    }
}
