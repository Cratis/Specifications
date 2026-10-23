// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace Cratis.Specifications.Resources;

/// <summary>
/// Represents a group of resources whose leases share one concurrency limit.
/// </summary>
/// <param name="key">The key of the group.</param>
/// <param name="maxConcurrency">The number of leases that may be held at once, or 0 for no limit.</param>
[SuppressMessage("Design", "CA1001", Justification = "The semaphore never allocates a wait handle, and a group lives until the test session ends.")]
[Experimental(SpecificationResources.DiagnosticId)]
internal sealed class ResourceGroup(string key, int maxConcurrency)
{
    readonly SemaphoreSlim? _slots = maxConcurrency > 0 ? new(maxConcurrency, maxConcurrency) : null;

    /// <summary>
    /// Gets the key of the group.
    /// </summary>
    public string Key { get; } = key;

    /// <summary>
    /// Gets the number of leases that may be held at once, or 0 for no limit.
    /// </summary>
    public int MaxConcurrency { get; } = maxConcurrency;

    /// <summary>
    /// Gets a value indicating whether the number of leases is limited.
    /// </summary>
    public bool IsLimited => _slots is not null;

    /// <summary>
    /// Waits for a free lease slot.
    /// </summary>
    /// <param name="cancellationToken">Cancels the wait.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task Enter(CancellationToken cancellationToken) => _slots?.WaitAsync(cancellationToken) ?? Task.CompletedTask;

    /// <summary>
    /// Frees a lease slot.
    /// </summary>
    public void Exit() => _slots?.Release();
}
