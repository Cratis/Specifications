// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NUnit.Framework;

namespace Cratis.Specifications;

/// <summary>
/// Makes a failed "Should*" assertion surface as an NUnit <see cref="AssertionException"/>.
/// </summary>
/// <remarks>
/// Instantiated by name from the core assertions on the first failure; shared by the NUnit 4 and NUnit 5 packages.
/// </remarks>
sealed class AssertionFailureFactory : IAssertionFailureFactory
{
    /// <inheritdoc/>
    public Exception Create(string message) => new AssertionException(message);
}
