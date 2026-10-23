// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
        SpecificationAssert.InRange(actual, low, high);
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
        SpecificationAssert.NotInRange(actual, low, high);
    }

    /// <summary>
    /// Assert that a value is greater than the other.
    /// </summary>
    /// <param name="left">Left value.</param>
    /// <param name="right">Right value.</param>
    public static void ShouldBeGreaterThan(this IComparable left, IComparable right)
    {
        if (left is null) SpecificationAssert.FailActualWasNull("ShouldBeGreaterThan", "a value greater than the other value");
        SpecificationAssert.True(left.CompareTo(right) > 0, $"{left} should be greater than {right}");
    }

    /// <summary>
    /// Assert that a value is greater or equal than the other.
    /// </summary>
    /// <param name="left">Left value.</param>
    /// <param name="right">Right value.</param>
    public static void ShouldBeGreaterThanOrEqual(this IComparable left, IComparable right)
    {
        if (left is null) SpecificationAssert.FailActualWasNull("ShouldBeGreaterThanOrEqual", "a value greater than or equal to the other value");
        SpecificationAssert.True(left.CompareTo(right) >= 0, $"{left} should be greater than or equal to {right}");
    }

    /// <summary>
    /// Assert that a value is less than the other.
    /// </summary>
    /// <param name="left">Left value.</param>
    /// <param name="right">Right value.</param>
    public static void ShouldBeLessThan(this IComparable left, IComparable right)
    {
        if (left is null) SpecificationAssert.FailActualWasNull("ShouldBeLessThan", "a value less than the other value");
        SpecificationAssert.True(left.CompareTo(right) < 0, $"{left} should be less than {right}");
    }

    /// <summary>
    /// Assert that a value is less than or equal than the other.
    /// </summary>
    /// <param name="left">Left value.</param>
    /// <param name="right">Right value.</param>
    public static void ShouldBeLessThanOrEqual(this IComparable left, IComparable right)
    {
        if (left is null) SpecificationAssert.FailActualWasNull("ShouldBeLessThanOrEqual", "a value less than or equal to the other value");
        SpecificationAssert.True(left.CompareTo(right) <= 0, $"{left} should be less than or equal to {right}");
    }
}
