using Xunit;

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
        Assert.Null(actual);
    }

    /// <summary>
    /// Assert that an object is not null.
    /// </summary>
    /// <param name="actual">Actual value.</param>
    public static void ShouldNotBeNull(this object actual)
    {
        Assert.NotNull(actual);
    }

    /// <summary>
    /// Assert that a boolean is false.
    /// </summary>
    /// <param name="actual">Actual value.</param>
    public static void ShouldBeFalse(this bool actual)
    {
        Assert.False(actual);
    }

    /// <summary>
    /// Assert that a boolean is true.
    /// </summary>
    /// <param name="actual">Actual value.</param>
    public static void ShouldBeTrue(this bool actual)
    {
        Assert.True(actual);
    }

    /// <summary>
    /// Assert that two objects are equal.
    /// </summary>
    /// <param name="actual">Actual value.</param>
    /// <param name="expected">Expected value.</param>
    /// <typeparam name="T">Type of object.</typeparam>
    public static void ShouldEqual<T>(this T actual, T expected)
    {
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Assert that two objects are not equal.
    /// </summary>
    /// <param name="actual">Actual value.</param>
    /// <param name="expected">Expected value.</param>
    /// <typeparam name="T">Type of object.</typeparam>
    public static void ShouldNotEqual<T>(this T actual, T expected)
    {
        Assert.NotEqual(expected, actual);
    }

    /// <summary>
    /// Assert that two objects are the same.
    /// </summary>
    /// <param name="actual">Actual value.</param>
    /// <param name="expected">Expected value.</param>
    public static void ShouldBeSame(this object actual, object expected)
    {
        Assert.Same(expected, actual);
    }

    /// <summary>
    /// Assert that two objects are not the same.
    /// </summary>
    /// <param name="actual">Actual value.</param>
    /// <param name="expected">Expected value.</param>
    public static void ShouldNotBeSame(this object actual, object expected)
    {
        Assert.NotSame(expected, actual);
    }

    /// <summary>
    /// Assert that two objects are not similar - a non strict equal. <see cref="Assert.NotStrictEqual{T}(T, T)"/>.
    /// </summary>
    /// <param name="actual">Actual value.</param>
    /// <param name="expected">Expected value.</param>
    /// <typeparam name="T">Type of object.</typeparam>
    public static void ShouldNotBeSimilar<T>(this T actual, T expected)
    {
        Assert.NotStrictEqual(expected, actual);
    }

    /// <summary>
    /// Assert that an object matches - based on a callback making the decision.
    /// </summary>
    /// <param name="actual">Actual value.</param>
    /// <param name="expected">Callback deciding what is expected.</param>
    /// <typeparam name="T">Type of object.</typeparam>
    public static void ShouldMatch<T>(this T actual, Func<T, bool> expected)
    {
        Assert.True(expected(actual));
    }
}
