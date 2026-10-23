// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications;

/// <summary>
/// Represents the exception thrown when a "Should*" assertion in a specification does not hold.
/// </summary>
/// <param name="message">The message describing the failed assertion, including the expected and actual values where applicable.</param>
public sealed class SpecificationAssertionException(string message) : Exception(message);
