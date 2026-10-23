// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Cratis.Specifications.Resources;

/// <summary>
/// Releases the leases a specification holds when its scope ends, after Destroy.
/// </summary>
/// <remarks>
/// Registered when the assembly loads, so every specification instance gets a scope to lease in: a lease is taken
/// during Establish, after the adapter decided whether to observe the instance. It is the first observer, so the
/// lease release is reported before the other observers see the scope end.
/// </remarks>
[Experimental(SpecificationResources.DiagnosticId)]
internal sealed class ResourceLeaseObserver : ISpecificationObserver
{
    /// <inheritdoc/>
    public ValueTask ScopeStarting(SpecificationScope scope) => default;

    /// <inheritdoc/>
    public void PhaseCompleted(SpecificationScope scope, SpecificationPhase phase, long startTimestamp, long endTimestamp, Exception? failure)
    {
    }

    /// <inheritdoc/>
    public ValueTask ScopeEnded(SpecificationScope scope) => ScopeLeases.Release(scope);

#pragma warning disable CA2255 // The observer must be registered before the first specification runs, from the core assembly itself.
    [ModuleInitializer]
    internal static void Register() => SpecificationObservers.Add(new ResourceLeaseObserver());
#pragma warning restore CA2255
}
