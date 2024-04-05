using NUnit.Framework;

namespace Cratis.Specifications;

/// <summary>
/// Holds extension methods for fluent "Should*" assertions related to strings.
/// </summary>
public static class ShouldStringExtensions
{
#if NET6_0_OR_GREATER
    /// <summary>
    /// Assert that a string contains an expected substring.
    /// </summary>
    /// <param name="actual">Actual string to assert.</param>
    /// <param name="expectedSubstring">Expected substring.</param>
    /// <param name="comparisonType">Optional <see cref="StringComparison">comparison type</see>.</param>
    public static void ShouldContain(this string actual, string expectedSubstring, StringComparison comparisonType = StringComparison.CurrentCulture)
    {
        Assert.That(() => actual.Contains(expectedSubstring, comparisonType), Is.True);
    }

    /// <summary>
    /// Assert that a string does not contain an expected substring.
    /// </summary>
    /// <param name="actual">Actual string to assert.</param>
    /// <param name="expectedSubstring">Not expected substring.</param>
    /// <param name="comparisonType">Optional <see cref="StringComparison">comparison type</see>.</param>
    public static void ShouldNotContain(this string actual, string expectedSubstring, StringComparison comparisonType = StringComparison.CurrentCulture)
    {
        Assert.That(() => actual.Contains(expectedSubstring, comparisonType), Is.False);
    }
#else
    /// <summary>
    /// Assert that a string contains an expected substring.
    /// </summary>
    /// <param name="actual">Actual string to assert.</param>
    /// <param name="expectedSubstring">Expected substring.</param>
    public static void ShouldContain(this string actual, string expectedSubstring)
    {
        Assert.That(() => actual.Contains(expectedSubstring), Is.True);
    }

    /// <summary>
    /// Assert that a string does not contain an expected substring.
    /// </summary>
    /// <param name="actual">Actual string to assert.</param>
    /// <param name="expectedSubstring">Not expected substring.</param>
    public static void ShouldNotContain(this string actual, string expectedSubstring)
    {
        Assert.That(() => actual.Contains(expectedSubstring), Is.False);
    }
#endif
}
