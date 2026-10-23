// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace Cratis.Specifications.Resources;

/// <summary>
/// Releases the leases a specification holds when its scope ends, after Destroy.
/// </summary>
/// <remarks>
/// Registered by <see cref="SpecificationResourcesHook.Enable"/> when the test assembly loads, and only for a test
/// project that opted in, so every specification instance of that project gets a scope to lease in.
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
}
