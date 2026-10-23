// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace Cratis.Specifications.Resources;

/// <summary>
/// Holds the leases a specification scope holds, and releases them when the scope ends.
/// </summary>
[Experimental(SpecificationResources.DiagnosticId)]
internal sealed class ScopeLeases
{
    static readonly ConditionalWeakTable<SpecificationScope, ScopeLeases> _scopes = [];

    readonly List<(ResourceGroup Group, IAsyncDisposable Lease)> _held = [];

    /// <summary>
    /// Gets the leases of a scope, creating the holder on first use.
    /// </summary>
    /// <param name="scope">The scope.</param>
    /// <returns>The leases of the scope.</returns>
    public static ScopeLeases For(SpecificationScope scope) => _scopes.GetValue(scope, static _ => new());

    /// <summary>
    /// Releases every lease of a scope, the most recent first, freeing each lease slot even when releasing fails.
    /// </summary>
    /// <param name="scope">The scope.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation; faulted with the release failures.</returns>
    public static ValueTask Release(SpecificationScope scope)
    {
        if (!_scopes.TryGetValue(scope, out var leases)) return default;
        _scopes.Remove(scope);
        return leases.ReleaseAll(scope);
    }

    /// <summary>
    /// Throws when leasing from a group now could deadlock against another scope.
    /// </summary>
    /// <param name="group">The group to lease from.</param>
    /// <exception cref="SpecificationResourceOrderException">When the scope holds a lease on a limited group whose key is not ordinally before the group's.</exception>
    public void EnsureInOrder(ResourceGroup group)
    {
        if (!group.IsLimited) return;
        lock (_held)
        {
            foreach (var (held, _) in _held)
            {
                if (held.IsLimited && string.CompareOrdinal(held.Key, group.Key) >= 0) throw new SpecificationResourceOrderException(held.Key, group.Key);
            }
        }
    }

    /// <summary>
    /// Adds a lease the scope now holds.
    /// </summary>
    /// <param name="group">The group whose slot the lease holds.</param>
    /// <param name="lease">The lease.</param>
    public void Add(ResourceGroup group, IAsyncDisposable lease)
    {
        lock (_held) _held.Add((group, lease));
    }

    async ValueTask ReleaseAll(SpecificationScope scope)
    {
        (ResourceGroup Group, IAsyncDisposable Lease)[] held;
        lock (_held) held = [.. _held];

        List<Exception>? failures = null;
        for (var index = held.Length - 1; index >= 0; index--)
        {
            var (group, lease) = held[index];
            try
            {
                await scope.Run(SpecificationPhase.LeaseRelease, _ => lease.DisposeAsync());
            }
            catch (Exception ex)
            {
                (failures ??= []).Add(ex);
            }
            finally
            {
                group.Exit();
            }
        }

        if (failures is null) return;
        if (failures.Count == 1) ExceptionDispatchInfo.Throw(failures[0]);
        throw new AggregateException(failures);
    }
}
