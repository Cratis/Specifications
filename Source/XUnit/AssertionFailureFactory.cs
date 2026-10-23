// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit.Sdk;

namespace Cratis.Specifications;

/// <summary>
/// Makes a failed "Should*" assertion surface as an xUnit <see cref="XunitException"/>.
/// </summary>
/// <remarks>
/// Instantiated by name from the core assertions on the first failure; shared by the xUnit v2 and v3 packages.
/// </remarks>
sealed class AssertionFailureFactory : IAssertionFailureFactory
{
    /// <inheritdoc/>
    public Exception Create(string message) => new XunitException(message);
}
