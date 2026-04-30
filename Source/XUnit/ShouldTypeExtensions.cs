// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
#pragma warning disable xUnit2032
        Assert.IsAssignableFrom<T>(actual);
#pragma warning restore xUnit2032
    }

    /// <summary>
    /// Asserts that an object is assignable from a specific type.
    /// </summary>
    /// <param name="actual">Object to assert.</param>
    /// <param name="expected">Type it should be assignable from.</param>
    public static void ShouldBeAssignableFrom(this object actual, Type expected)
    {
#pragma warning disable xUnit2032
        Assert.IsAssignableFrom(expected, actual);
#pragma warning restore xUnit2032
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
