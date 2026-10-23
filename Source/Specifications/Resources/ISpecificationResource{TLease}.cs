// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace Cratis.Specifications.Resources;

/// <summary>
/// Defines a shared resource that gives each specification an isolated lease on it, such as a fresh database on a
/// shared database server.
/// </summary>
/// <typeparam name="TLease">The type of the lease, disposed when the specification that holds it has finished.</typeparam>
/// <remarks>
/// A specification leases through <see cref="SpecificationResources.Lease{T, TLease}(CancellationToken)"/> from its
/// Establish or Because. The lease is disposed after Destroy, the most recent lease first.
/// <see cref="SpecificationResourceAttribute.MaxConcurrency"/> limits how many specifications hold a lease at once.
/// </remarks>
[Experimental(SpecificationResources.DiagnosticId)]
public interface ISpecificationResource<TLease> : ISpecificationResource
    where TLease : IAsyncDisposable
{
    /// <summary>
    /// Provisions, or resets, an isolated part of the resource for one specification.
    /// </summary>
    /// <param name="context">The specification the lease is for.</param>
    /// <param name="cancellationToken">The cancellation token the specification passed when leasing.</param>
    /// <returns>The lease.</returns>
    /// <remarks>An exception thrown here fails only the specification that asked for the lease.</remarks>
    ValueTask<TLease> Lease(SpecificationLeaseContext context, CancellationToken cancellationToken);
}
