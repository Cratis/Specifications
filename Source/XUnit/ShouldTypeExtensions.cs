using Xunit;

namespace Cratis.Specifications;

/// <summary>
/// Holds extension methods for fluent "Should*" assertions related to types.
/// </summary>
public static class ShouldTypeExtensions
{
    /// <summary>
    /// Asserts that an object is assignable from a specific type.
    /// </summary>
    /// <param name="actual">Object to assert.</param>
    /// <typeparam name="T">Type it should be assignable from.</typeparam>
    public static void ShouldBeAssignableFrom<T>(this object actual)
    {
        Assert.IsAssignableFrom<T>(actual);
    }

    /// <summary>
    /// Asserts that an object is assignable from a specific type.
    /// </summary>
    /// <param name="actual">Object to assert.</param>
    /// <param name="expected">Type it should be assignable from.</param>
    public static void ShouldBeAssignableFrom(this object actual, Type expected)
    {
        Assert.IsAssignableFrom(expected, actual);
    }

    /// <summary>
    /// Assert that an object is of an exact type.
    /// </summary>
    /// <param name="actual">Object to assert.</param>
    /// <typeparam name="T">Type it should be.</typeparam>
    public static void ShouldBeOfExactType<T>(this object actual)
    {
        Assert.IsType<T>(actual);
    }
}
