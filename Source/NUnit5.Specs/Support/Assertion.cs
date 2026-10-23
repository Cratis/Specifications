// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications;

/// <summary>
/// Helpers for asserting that an assertion fails.
/// </summary>
public static class Assertion
{
    /// <summary>
    /// Asserts that the given assertion fails. Takes an <see cref="Action"/> so the call resolves identically on NUnit 4 and NUnit 5.
    /// </summary>
    /// <param name="assertion">The assertion that is expected to fail.</param>
    public static void ShouldFail(Action assertion) => Assert.Throws<AssertionException>(assertion);
}
