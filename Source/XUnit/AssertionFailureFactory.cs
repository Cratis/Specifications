// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit.Sdk;

using System.Runtime.CompilerServices;

namespace Cratis.Specifications;

/// <summary>
/// Makes a failed "Should*" assertion surface as an xUnit <see cref="XunitException"/>.
/// </summary>
/// <remarks>
/// Registered with the core assertions by a module initializer; shared by the xUnit v2 and v3 packages.
/// </remarks>
sealed class AssertionFailureFactory : IAssertionFailureFactory
{
    /// <summary>
    /// Registers the factory with the core assertions when the package assembly loads.
    /// </summary>
#pragma warning disable CA2255 // The package registers its failure factory when its assembly loads, before the first assertion runs.
    [ModuleInitializer]
    internal static void Register() => AssertionFailures.Register(new AssertionFailureFactory());
#pragma warning restore CA2255

    /// <inheritdoc/>
    public Exception Create(string message) => new XunitException(message);
}
