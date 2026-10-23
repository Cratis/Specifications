// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using NUnit.Framework;

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
        Assert.That(collection, Is.EquivalentTo(expected));
    }

    /// <summary>
    /// Assert that a collection only contains the expected elements - based on params.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    /// <param name="expected">Expected values.</param>
    /// <typeparam name="T">Type of element.</typeparam>
    public static void ShouldContainOnly<T>(this IEnumerable<T> collection, params T[] expected)
    {
        if (collection is null) NullActual.Fail("ShouldContainOnly", "a collection containing only the expected items");
        Assert.That(collection, Is.EquivalentTo(expected));
    }

    /// <summary>
    /// Assert that a collection contains exactly only the expected elements in the same sequence.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    /// <param name="expected">Expected values.</param>
    /// <typeparam name="T">Type of element.</typeparam>
    public static void ShouldEqual<T>(this IEnumerable<T> collection, IEnumerable<T> expected)
    {
        Assert.That(collection, Is.EqualTo(expected));
    }

    /// <summary>
    /// Assert that a collection contains exactly only the expected elements in the same sequence - based on params.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    /// <param name="expected">Expected values.</param>
    /// <typeparam name="T">Type of element.</typeparam>
    public static void ShouldEqual<T>(this IEnumerable<T> collection, params T[] expected)
    {
#if NUNIT5
        Assert.That(collection, Is.EqualTo(expected));
#else
        // The NUnit 4 package has always compared without regard to order. Kept for compatibility;
        // the NUnit 5 package compares the sequence, like the xUnit and TUnit packages.
        Assert.That(collection, Is.EquivalentTo(expected));
#endif
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
        Assert.That(expected.Except(collection), Is.Empty);
    }

    /// <summary>
    /// Assert that a collection contains all the expected elements - based on params.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    /// <param name="expected">Expected elements as params.</param>
    /// <typeparam name="T">Type of element.</typeparam>
    public static void ShouldContain<T>(this IEnumerable<T> collection, params T[] expected)
    {
        if (collection is null) NullActual.Fail("ShouldContain", "the actual value to contain the expected item");
        Assert.That(expected.Except(collection), Is.Empty);
    }

    /// <summary>
    /// Assert that a dictionary contains a specific key.
    /// </summary>
    /// <param name="actual">Dictionary to assert.</param>
    /// <param name="expected">Expected key.</param>
    /// <typeparam name="TKey">Type of key.</typeparam>
    /// <typeparam name="TValue">Type of value.</typeparam>
    public static void ShouldContain<TKey, TValue>(this IDictionary<TKey, TValue> actual, TKey expected)
    {
        if (actual is null) NullActual.Fail("ShouldContain", "the actual value to contain the expected item");
        Assert.That(actual, Contains.Key(expected!));
    }

    /// <summary>
    /// Assert that a dictionary does not contain a specific key.
    /// </summary>
    /// <param name="actual">Dictionary to assert.</param>
    /// <param name="expected">Not expected key.</param>
    /// <typeparam name="TKey">Type of key.</typeparam>
    /// <typeparam name="TValue">Type of value.</typeparam>
    public static void ShouldNotContain<TKey, TValue>(this IDictionary<TKey, TValue> actual, TKey expected)
    {
        if (actual is null) NullActual.Fail("ShouldNotContain", "the actual value not to contain the specified item");
        Assert.That(actual.Keys, Has.No.Member(expected));
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
        // NUnit 5 declares the predicate as Predicate<T?>; the null-forgiving operator is needed there but redundant on NUnit 4.
#pragma warning disable IDE0370
        Assert.That(collection, Has.Some.Matches<T>(item => filter(item!)));
#pragma warning restore IDE0370
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
        Assert.That(collection.Where(i => filter(i)), Has.One.Items);
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
        // For some reason, this does not work: Assert.That(collection, Has.No.Matches(filter));
        Assert.That(collection.Any(t => filter(t)), Is.False);
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
        Assert.That(collection, Contains.Item(expected));
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
        Assert.That(collection, Has.No.Member(expected));
    }

    /// <summary>
    /// Assert that all items in a collection are conforming based on the decision of a callback.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    /// <param name="filter">The filter.</param>
    /// <typeparam name="T">Type of element.</typeparam>
    public static void ShouldEachConformTo<T>(this IEnumerable<T> collection, Predicate<T> filter)
    {
        if (collection is null) NullActual.Fail("ShouldEachConformTo", "every item to conform to the filter");
        // NUnit 5 declares the predicate as Predicate<T?>; the null-forgiving operator is needed there but redundant on NUnit 4.
#pragma warning disable IDE0370
        Assert.That(collection, Has.All.Matches<T>(item => filter(item!)));
#pragma warning restore IDE0370
    }

    /// <summary>
    /// Assert that a collection is empty.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    public static void ShouldBeEmpty(this IEnumerable collection)
    {
        if (collection is null) NullActual.Fail("ShouldBeEmpty", "an empty collection");
        Assert.That(collection, Is.Empty);
    }

    /// <summary>
    /// Assert that a collection is not empty.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    public static void ShouldNotBeEmpty(this IEnumerable collection)
    {
        if (collection is null) NullActual.Fail("ShouldNotBeEmpty", "a non-empty collection");
        Assert.That(collection, Is.Not.Empty);
    }

    /// <summary>
    /// Assert that a collection has a single item.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    public static void ShouldContainSingleItem(this IEnumerable collection)
    {
        if (collection is null) NullActual.Fail("ShouldContainSingleItem", "a collection with a single item");
        Assert.That(collection, Has.One.Items);
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
        Assert.That(collection, Is.EqualTo(expected));
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
        if (collection is null) NullActual.Fail("ShouldEqualInAnyOrder", "a collection equal to the expected items");
        Assert.That(collection, Is.EquivalentTo(expected));
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
}
