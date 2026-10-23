// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace Cratis.Specifications.Resources;

/// <summary>
/// Gives specifications access to resources shared for the whole test session, and to isolated leases on them.
/// </summary>
/// <remarks>
/// <para>
/// Ask from Establish:
/// <code>
/// async Task Establish()
/// {
///     _server = await SpecificationResources.Get&lt;PostgresServer&gt;();
///     _database = await SpecificationResources.Lease&lt;PostgresServer, FreshDatabase&gt;();
/// }
/// </code>
/// </para>
/// <para>
/// Nothing starts until a specification asks for it, so skipped and unselected specifications start nothing.
/// Every resource started is stopped when the test session ends; a failure to stop is reported by the test framework
/// as a cleanup failure of the run.
/// </para>
/// </remarks>
[Experimental(DiagnosticId)]
public static class SpecificationResources
{
    /// <summary>
    /// The diagnostic id of the experimental shared resources API.
    /// </summary>
    public const string DiagnosticId = "CRSPECEXP01";

    static ResourceRegistry _registry = new();

    /// <summary>
    /// Gets the registry the static API works against.
    /// </summary>
    internal static ResourceRegistry Registry => Volatile.Read(ref _registry);

    /// <summary>
    /// Gets a shared resource, creating and starting it on first use.
    /// </summary>
    /// <typeparam name="T">The type of the resource.</typeparam>
    /// <param name="cancellationToken">Cancels waiting for the resource to start; the start itself carries on for others.</param>
    /// <returns>The started resource.</returns>
    public static ValueTask<T> Get<T>(CancellationToken cancellationToken = default)
        where T : class, ISpecificationResource, new() =>
        Registry.Get(ResourceFactory<T>.Create, cancellationToken);

    /// <summary>
    /// Gets a shared resource, creating it with the given factory and starting it on first use.
    /// </summary>
    /// <typeparam name="T">The type of the resource.</typeparam>
    /// <param name="factory">Creates the resource; only used when the resource has not been created yet.</param>
    /// <param name="cancellationToken">Cancels waiting for the resource to start; the start itself carries on for others.</param>
    /// <returns>The started resource.</returns>
    public static ValueTask<T> Get<T>(Func<T> factory, CancellationToken cancellationToken = default)
        where T : class, ISpecificationResource =>
        Registry.Get(factory, cancellationToken);

    /// <summary>
    /// Leases an isolated part of a shared resource for the current specification, starting the resource on first use.
    /// </summary>
    /// <typeparam name="T">The type of the resource.</typeparam>
    /// <typeparam name="TLease">The type of the lease.</typeparam>
    /// <param name="cancellationToken">Cancels waiting for the resource or for a free lease.</param>
    /// <returns>The lease, disposed after the specification's Destroy.</returns>
    /// <exception cref="InvalidOperationException">When called outside Establish or Because.</exception>
    /// <exception cref="SpecificationResourceOrderException">When leasing would break the ordinal key order of limited groups.</exception>
    /// <exception cref="SpecificationCancelledException">When cancelled while waiting.</exception>
    public static ValueTask<TLease> Lease<T, TLease>(CancellationToken cancellationToken = default)
        where T : class, ISpecificationResource<TLease>, new()
        where TLease : IAsyncDisposable =>
        Registry.Lease<T, TLease>(ResourceFactory<T>.Create, cancellationToken);

    /// <summary>
    /// Leases an isolated part of a shared resource for the current specification, creating the resource with the
    /// given factory and starting it on first use.
    /// </summary>
    /// <typeparam name="T">The type of the resource.</typeparam>
    /// <typeparam name="TLease">The type of the lease.</typeparam>
    /// <param name="factory">Creates the resource; only used when the resource has not been created yet.</param>
    /// <param name="cancellationToken">Cancels waiting for the resource or for a free lease.</param>
    /// <returns>The lease, disposed after the specification's Destroy.</returns>
    /// <exception cref="InvalidOperationException">When called outside Establish or Because.</exception>
    /// <exception cref="SpecificationResourceOrderException">When leasing would break the ordinal key order of limited groups.</exception>
    /// <exception cref="SpecificationCancelledException">When cancelled while waiting.</exception>
    public static ValueTask<TLease> Lease<T, TLease>(Func<T> factory, CancellationToken cancellationToken = default)
        where T : class, ISpecificationResource<TLease>
        where TLease : IAsyncDisposable =>
        Registry.Lease<T, TLease>(factory, cancellationToken);

    /// <summary>
    /// Shares a resource that something else owns and has started. It is never started, stopped or disposed here.
    /// </summary>
    /// <typeparam name="T">The type the resource is asked for by.</typeparam>
    /// <param name="external">The resource.</param>
    /// <exception cref="InvalidOperationException">When a resource of the type is already registered.</exception>
    public static void Borrow<T>(T external)
        where T : class, ISpecificationResource =>
        Registry.Borrow(external);

    /// <summary>
    /// Replaces the registry the static API works against. For specifications of the shared resources themselves.
    /// </summary>
    /// <param name="registry">The registry to use.</param>
    /// <returns>The registry that was in use.</returns>
    internal static ResourceRegistry Use(ResourceRegistry registry) => Interlocked.Exchange(ref _registry, registry);

    static class ResourceFactory<T>
        where T : class, new()
    {
        public static readonly Func<T> Create = static () => new T();
    }
}
