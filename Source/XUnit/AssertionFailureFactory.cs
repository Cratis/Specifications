// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit.Sdk;

using System.ComponentModel;
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
    /// <inheritdoc/>
    public Exception Create(string message) => new XunitException(message);
}

/// <summary>
/// Registers the xUnit assertion failure factory with the core assertions.
/// </summary>
/// <remarks>
/// Runs when the package assembly loads and again from a module initializer the package compiles into the test assembly,
/// so registration never depends on which assembly loaded first. Not meant to be called from specifications.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class AssertionFailureRegistration
{
    /// <summary>
    /// Registers the factory. Safe to call any number of times.
    /// </summary>
#pragma warning disable CA2255 // The package registers its failure factory when its assembly loads, before the first assertion runs.
    [ModuleInitializer]
    public static void Register() => AssertionFailures.Register(new AssertionFailureFactory());
#pragma warning restore CA2255
}
