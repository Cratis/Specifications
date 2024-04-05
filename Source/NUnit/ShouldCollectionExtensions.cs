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
        Assert.That(collection, Is.EquivalentTo(expected));
    }

    /// <summary>
    /// Assert that a collection contains all the expected elements.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    /// <param name="expected">Expected elements.</param>
    /// <typeparam name="T">Type of element.</typeparam>
    public static void ShouldContain<T>(this IEnumerable<T> collection, IEnumerable<T> expected)
    {
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
        Assert.That(collection, Has.Some.Matches(filter));
    }

    /// <summary>
    /// Assert that a collection contains ONE specific element based on a predicate filter.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    /// <param name="filter">Filter to apply.</param>
    /// <typeparam name="T">Type of element.</typeparam>
    public static void ShouldContainSingleMatching<T>(this IEnumerable<T> collection, Predicate<T> filter)
    {
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
        // For some reason, this does not work: Assert.That(collection, Has.No.Matches(filter));
        Assert.That(() => collection.Any(t => filter(t)), Is.False);
    }

    /// <summary>
    /// Assert that a collection contains a specific element.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    /// <param name="expected">Expected element.</param>
    /// <typeparam name="T">Type of element.</typeparam>
    public static void ShouldContain<T>(this IEnumerable<T> collection, T expected)
    {
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
        Assert.That(collection, Has.All.Matches(filter));
    }

    /// <summary>
    /// Assert that a collection is empty.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    public static void ShouldBeEmpty(this IEnumerable collection)
    {
        Assert.That(collection, Is.Empty);
    }

    /// <summary>
    /// Assert that a collection is not empty.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    public static void ShouldNotBeEmpty(this IEnumerable collection)
    {
        Assert.That(collection, Is.Not.Empty);
    }

    /// <summary>
    /// Assert that a collection has a single item.
    /// </summary>
    /// <param name="collection">Collection to assert.</param>
    public static void ShouldContainSingleItem(this IEnumerable collection)
    {
        Assert.That(collection, Has.One.Items);
    }
}
