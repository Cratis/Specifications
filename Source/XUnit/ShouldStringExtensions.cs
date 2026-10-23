// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace Cratis.Specifications;

/// <summary>
/// Holds extension methods for fluent "Should*" assertions related to strings.
/// </summary>
public static class ShouldStringExtensions
{
    /// <summary>
    /// Assert that a string contains an expected substring.
    /// </summary>
    /// <param name="actual">Actual string to assert.</param>
    /// <param name="expectedSubstring">Expected substring.</param>
    /// <param name="comparisonType">Optional <see cref="StringComparison">comparison type</see>.</param>
    public static void ShouldContain(this string actual, string expectedSubstring, StringComparison comparisonType = StringComparison.CurrentCulture)
    {
        if (actual is null) NullActual.Fail("ShouldContain", "the actual value to contain the expected item");
        Assert.Contains(expectedSubstring, actual, comparisonType);
    }

    /// <summary>
    /// Assert that a string does not contain an expected substring.
    /// </summary>
    /// <param name="actual">Actual string to assert.</param>
    /// <param name="expectedSubstring">Not expected substring.</param>
    /// <param name="comparisonType">Optional <see cref="StringComparison">comparison type</see>.</param>
    public static void ShouldNotContain(this string actual, string expectedSubstring, StringComparison comparisonType = StringComparison.CurrentCulture)
    {
        if (actual is null) NullActual.Fail("ShouldNotContain", "the actual value not to contain the specified item");
        Assert.DoesNotContain(expectedSubstring, actual, comparisonType);
    }
}
