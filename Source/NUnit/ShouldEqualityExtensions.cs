using NUnit.Framework;

namespace Cratis.Specifications;

/// <summary>
/// Holds extension methods for fluent "Should*" assertions related to equality checks.
/// </summary>
public static class ShouldEqualityExtensions
{
    /// <summary>
    /// Assert that an object is null.
    /// </summary>
    /// <param name="actual">Actual value.</param>
    public static void ShouldBeNull(this object actual)
    {
        Assert.That(actual, Is.Null);
    }

    /// <summary>
    /// Assert that an object is not null.
    /// </summary>
    /// <param name="actual">Actual value.</param>
    public static void ShouldNotBeNull(this object actual)
    {
        Assert.That(actual, Is.Not.Null);
    }

    /// <summary>
    /// Assert that a boolean is false.
    /// </summary>
    /// <param name="actual">Actual value.</param>
    public static void ShouldBeFalse(this bool actual)
    {
        Assert.That(actual, Is.False);
    }

    /// <summary>
    /// Assert that a boolean is true.
    /// </summary>
    /// <param name="actual">Actual value.</param>
    public static void ShouldBeTrue(this bool actual)
    {
        Assert.That(actual, Is.True);
    }

    /// <summary>
    /// Assert that two objects are equal.
    /// </summary>
    /// <param name="actual">Actual value.</param>
    /// <param name="expected">Expected value.</param>
    /// <typeparam name="T">Type of object.</typeparam>
    public static void ShouldEqual<T>(this T actual, T expected)
    {
        Assert.That(actual, Is.EqualTo(expected));
    }

    /// <summary>
    /// Assert that two objects are not equal.
    /// </summary>
    /// <param name="actual">Actual value.</param>
    /// <param name="expected">Expected value.</param>
    /// <typeparam name="T">Type of object.</typeparam>
    public static void ShouldNotEqual<T>(this T actual, T expected)
    {
        Assert.That(actual, Is.Not.EqualTo(expected));
    }

    /// <summary>
    /// Assert that two objects are the same.
    /// </summary>
    /// <param name="actual">Actual value.</param>
    /// <param name="expected">Expected value.</param>
    public static void ShouldBeSame(this object actual, object expected)
    {
        Assert.That(actual, Is.SameAs(expected));
    }

    /// <summary>
    /// Assert that two objects are not the same.
    /// </summary>
    /// <param name="actual">Actual value.</param>
    /// <param name="expected">Expected value.</param>
    public static void ShouldNotBeSame(this object actual, object expected)
    {
        Assert.That(actual, Is.Not.SameAs(expected));
    }

    /// <summary>
    /// Assert that two objects are not similar - a non strict equal.
    /// </summary>
    /// <param name="actual">Actual value.</param>
    /// <param name="expected">Expected value.</param>
    /// <typeparam name="T">Type of object.</typeparam>
    public static void ShouldNotBeSimilar<T>(this T actual, T expected)
    {
        Assert.That(actual, Is.Not.EqualTo(expected));
    }

    /// <summary>
    /// Assert that an object matches - based on a callback making the decision.
    /// </summary>
    /// <param name="actual">Actual value.</param>
    /// <param name="expected">Callback deciding what is expected.</param>
    /// <typeparam name="T">Type of object.</typeparam>
    public static void ShouldMatch<T>(this T actual, Predicate<T> expected)
    {
        Assert.That(expected(actual), Is.True);
    }
}
