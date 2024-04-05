using NUnit.Framework;

namespace Cratis.Specifications;

/// <summary>
/// Holds extension methods for fluent "Should*" assertions related to comparables.
/// </summary>
public static class ShouldComparableExtensions
{
    /// <summary>
    /// Assert that a value is within range.
    /// </summary>
    /// <param name="actual">Value to compare.</param>
    /// <param name="low">Lowest value in range.</param>
    /// <param name="high">Highest value in range.</param>
    /// <typeparam name="T">Type to compare.</typeparam>
    public static void ShouldBeInRange<T>(this T actual, T low, T high)
        where T : IComparable
    {
        Assert.That(actual, Is.InRange(low, high));
    }

    /// <summary>
    /// Assert that a value is not within range.
    /// </summary>
    /// <param name="actual">Value to compare.</param>
    /// <param name="low">Lowest value in range.</param>
    /// <param name="high">Highest value in range.</param>
    /// <typeparam name="T">Type to compare.</typeparam>
    public static void ShouldNotBeInRange<T>(this T actual, T low, T high)
        where T : IComparable
    {
        Assert.That(actual, Is.Not.InRange(low, high));
    }

    /// <summary>
    /// Assert that a value is greater than the other.
    /// </summary>
    /// <param name="left">Left value.</param>
    /// <param name="right">Right value.</param>
    public static void ShouldBeGreaterThan(this IComparable left, IComparable right)
    {
        Assert.That(left, Is.GreaterThan(right));
    }

    /// <summary>
    /// Assert that a value is greater or equal than the other.
    /// </summary>
    /// <param name="left">Left value.</param>
    /// <param name="right">Right value.</param>
    public static void ShouldBeGreaterThanOrEqual(this IComparable left, IComparable right)
    {
        Assert.That(left, Is.GreaterThanOrEqualTo(right));
    }

    /// <summary>
    /// Assert that a value is less than the other.
    /// </summary>
    /// <param name="left">Left value.</param>
    /// <param name="right">Right value.</param>
    public static void ShouldBeLessThan(this IComparable left, IComparable right)
    {
        Assert.That(left, Is.LessThan(right));
    }

    /// <summary>
    /// Assert that a value is less than or equal than the other.
    /// </summary>
    /// <param name="left">Left value.</param>
    /// <param name="right">Right value.</param>
    public static void ShouldBeLessThanOrEqual(this IComparable left, IComparable right)
    {
        Assert.That(left, Is.LessThanOrEqualTo(right));
    }
}
