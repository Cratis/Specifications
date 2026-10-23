// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Resources;

/// <summary>
/// A lease on a probe resource that records its release.
/// </summary>
/// <param name="name">The name of the resource the lease is on.</param>
/// <param name="context">The context the lease was taken with.</param>
/// <param name="log">The log to record to.</param>
/// <param name="releaseFailure">The exception to fail the release with, if any.</param>
internal sealed class ProbeLease(string name, SpecificationLeaseContext context, ResourceLog log, Exception releaseFailure) : IAsyncDisposable
{
    public SpecificationLeaseContext Context { get; } = context;

    public ValueTask DisposeAsync()
    {
        log.Add($"release {name}");
        log.LeaseReleased();
        return releaseFailure is null ? ValueTask.CompletedTask : ValueTask.FromException(releaseFailure);
    }
}
