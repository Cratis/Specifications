// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications;

/// <summary>
/// Defines a factory for the exception thrown when a "Should*" assertion does not hold.
/// </summary>
/// <remarks>
/// Each test framework package implements this so a failing assertion surfaces as the framework's own assertion exception.
/// </remarks>
interface IAssertionFailureFactory
{
    /// <summary>
    /// Creates the exception describing a failed assertion.
    /// </summary>
    /// <param name="message">The message describing the failed assertion.</param>
    /// <returns>The exception to throw.</returns>
    Exception Create(string message);
}
