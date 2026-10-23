// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace Cratis.Specifications.Resources;

/// <summary>
/// Represents one shared resource of a registry and the one start it gets.
/// </summary>
/// <param name="resource">The resource.</param>
/// <param name="owned">Whether the registry starts and stops the resource; false for a borrowed resource.</param>
/// <param name="group">The group the resource's leases belong to.</param>
/// <param name="stopTimeout">How long stopping the resource may take.</param>
[Experimental(SpecificationResources.DiagnosticId)]
internal sealed class ResourceEntry(ISpecificationResource resource, bool owned, ResourceGroup group, TimeSpan stopTimeout)
{
    readonly Lock _lock = new();
    Task? _start = owned ? null : Task.CompletedTask;

    /// <summary>
    /// Gets the resource.
    /// </summary>
    public ISpecificationResource Resource { get; } = resource;

    /// <summary>
    /// Gets the group the resource's leases belong to.
    /// </summary>
    public ResourceGroup Group { get; } = group;

    /// <summary>
    /// Gets how long stopping the resource may take.
    /// </summary>
    public TimeSpan StopTimeout { get; } = stopTimeout;

    /// <summary>
    /// Gets the start of the resource, beginning it with the given starter the first time.
    /// </summary>
    /// <param name="starter">Starts the resource; called at most once.</param>
    /// <returns>The start, completed once the resource has started and faulted with the same exception for every caller when it failed.</returns>
    public Task Started(Func<ResourceEntry, Task> starter)
    {
        var start = Volatile.Read(ref _start);
        if (start is not null) return start;

        TaskCompletionSource begun;
        lock (_lock)
        {
            if (_start is not null) return _start;
            begun = new(TaskCreationOptions.RunContinuationsAsynchronously);
            Volatile.Write(ref _start, begun.Task);
        }

        _ = Complete(begun, starter);
        return begun.Task;
    }

    async Task Complete(TaskCompletionSource begun, Func<ResourceEntry, Task> starter)
    {
        try
        {
            await starter(this);
            begun.SetResult();
        }
        catch (Exception ex)
        {
            begun.SetException(ex);
        }
    }
}
