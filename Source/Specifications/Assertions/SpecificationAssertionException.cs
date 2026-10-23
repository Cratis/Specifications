// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications;

/// <summary>
/// Represents the exception thrown when a "Should*" assertion in a specification does not hold.
/// </summary>
/// <param name="message">The message describing the failed assertion, including the expected and actual values where applicable.</param>
/// <param name="expected">The formatted expected value, when the assertion compares values.</param>
/// <param name="actual">The formatted actual value, when the assertion compares values.</param>
public sealed class SpecificationAssertionException(string message, string? expected, string? actual) : Exception(message)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SpecificationAssertionException"/> class.
    /// </summary>
    /// <param name="message">The message describing the failed assertion, including the expected and actual values where applicable.</param>
    public SpecificationAssertionException(string message)
        : this(message, null, null)
    {
    }

    /// <summary>
    /// Gets the formatted expected value; null when the assertion does not compare values.
    /// </summary>
    public string? Expected { get; } = expected;

    /// <summary>
    /// Gets the formatted actual value; null when the assertion does not compare values.
    /// </summary>
    public string? Actual { get; } = actual;
}
