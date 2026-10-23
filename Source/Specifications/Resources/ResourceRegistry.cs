// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Cratis.Specifications.Resources;

/// <summary>
/// Creates, starts, leases and stops the shared resources of a test session.
/// </summary>
/// <remarks>
/// Nothing is created or started before it is asked for. The first resource started registers the stop with the end
/// of the test session; stopping clears the registry, so a resource asked for afterwards starts again.
/// </remarks>
/// <param name="registerSessionEnd">Registers the stop with the end of the test session; defaults to <see cref="SpecificationSession.Register"/>.</param>
[Experimental(SpecificationResources.DiagnosticId)]
[SuppressMessage("Design", "CA1001", Justification = "The registry lives until the test session ends; Stop disposes the token source it replaces.")]
internal sealed class ResourceRegistry(Action<Func<ValueTask>>? registerSessionEnd = null)
{
    static long _leaseIds;

    readonly Action<Func<ValueTask>> _registerSessionEnd = registerSessionEnd ?? SpecificationSession.Register;
    readonly Lock _lock = new();
    Dictionary<Type, ResourceEntry> _entries = [];
    Dictionary<string, ResourceGroup> _groups = new(StringComparer.Ordinal);
    List<ResourceEntry> _started = [];
    List<Exception> _cleanupFailures = [];
    CancellationTokenSource _ending = new();
    bool _stopRegistered;

    /// <summary>
    /// Gets a resource, creating and starting it on first use.
    /// </summary>
    /// <typeparam name="T">The type of the resource.</typeparam>
    /// <param name="factory">Creates the resource when it has not been created yet.</param>
    /// <param name="cancellationToken">Cancels waiting for the start.</param>
    /// <returns>The started resource.</returns>
    public async ValueTask<T> Get<T>(Func<T> factory, CancellationToken cancellationToken)
        where T : class, ISpecificationResource
    {
        var entry = EntryFor(factory);
        await WaitUntilStarted(entry, nameof(Get), cancellationToken);
        return (T)entry.Resource;
    }

    /// <summary>
    /// Leases from a resource for the current specification scope, creating and starting the resource on first use.
    /// </summary>
    /// <typeparam name="T">The type of the resource.</typeparam>
    /// <typeparam name="TLease">The type of the lease.</typeparam>
    /// <param name="factory">Creates the resource when it has not been created yet.</param>
    /// <param name="cancellationToken">Cancels waiting for the start or for a free lease slot.</param>
    /// <returns>The lease, released when the scope ends.</returns>
    public async ValueTask<TLease> Lease<T, TLease>(Func<T> factory, CancellationToken cancellationToken)
        where T : class, ISpecificationResource<TLease>
        where TLease : IAsyncDisposable
    {
        var scope = SpecificationScope.Current ??
            throw new InvalidOperationException("Lease shared resources from the Establish or Because of a specification, so the lease can be released after Destroy.");
        var entry = EntryFor(factory);
        await WaitUntilStarted(entry, nameof(Lease), cancellationToken);

        var leases = ScopeLeases.For(scope);
        var group = entry.Group;
        leases.EnsureInOrder(group);
        if (group.IsLimited) await scope.Run(SpecificationPhase.AcquireWait, _ => new ValueTask(EnterGroup(group, cancellationToken)));

        try
        {
            var context = new SpecificationLeaseContext(scope.SpecificationType, scope.DisplayName, NextLeaseId());
            TLease lease = default!;
            await scope.Run(SpecificationPhase.Lease, async _ => lease = await ((T)entry.Resource).Lease(context, cancellationToken));
            leases.Add(group, lease);
            return lease;
        }
        catch
        {
            group.Exit();
            throw;
        }
    }

    /// <summary>
    /// Registers a resource that something else owns and has started.
    /// </summary>
    /// <typeparam name="T">The type the resource is asked for by.</typeparam>
    /// <param name="external">The resource.</param>
    public void Borrow<T>(T external)
        where T : class, ISpecificationResource
    {
        ArgumentNullException.ThrowIfNull(external);
        lock (_lock)
        {
            if (_entries.ContainsKey(typeof(T))) throw new InvalidOperationException($"A shared resource of type '{typeof(T).FullName}' is already registered.");
            _entries.Add(typeof(T), CreateEntry(typeof(T), external, owned: false));
        }
    }

    /// <summary>
    /// Stops every started resource, the last started first, and clears the registry.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation; faulted with every stop and cleanup failure.</returns>
    public async ValueTask Stop()
    {
        ResourceEntry[] started;
        List<Exception> failures;
        CancellationTokenSource ending;
        lock (_lock)
        {
            started = [.. _started];
            failures = _cleanupFailures;
            ending = _ending;
            _entries = [];
            _groups = new(StringComparer.Ordinal);
            _started = [];
            _cleanupFailures = [];
            _ending = new();
            _stopRegistered = false;
        }

        await ending.CancelAsync();
        ending.Dispose();

        for (var index = started.Length - 1; index >= 0; index--)
        {
            try
            {
                await StopResource(started[index]);
            }
            catch (Exception ex)
            {
                failures.Add(ex);
            }
        }

        if (failures.Count == 0) return;
        if (failures.Count == 1) ExceptionDispatchInfo.Throw(failures[0]);
        throw new AggregateException(failures);
    }

    static async Task EnterGroup(ResourceGroup group, CancellationToken cancellationToken)
    {
        try
        {
            await group.Enter(cancellationToken);
        }
        catch (OperationCanceledException ex) when (cancellationToken.IsCancellationRequested)
        {
            throw new SpecificationCancelledException(nameof(Lease), ex);
        }
    }

    static async Task StopResource(ResourceEntry entry)
    {
        var name = entry.Resource.GetType().FullName;
        var stopping = entry.Resource.DisposeAsync().AsTask();
        try
        {
            await stopping.WaitAsync(entry.StopTimeout);
        }
        catch (TimeoutException ex) when (!stopping.IsCompleted)
        {
            throw new TimeoutException($"Stopping the shared resource '{name}' did not finish within {entry.StopTimeout.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Stopping the shared resource '{name}' failed: {ex.Message}", ex);
        }
    }

    static string NextLeaseId() =>
        string.Create(CultureInfo.InvariantCulture, $"lease_{Environment.ProcessId}_{Interlocked.Increment(ref _leaseIds)}");

    async Task WaitUntilStarted(ResourceEntry entry, string operation, CancellationToken cancellationToken)
    {
        var start = entry.Started(Start);
        if (start.IsCompletedSuccessfully) return;

        try
        {
            await start.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException ex) when (cancellationToken.IsCancellationRequested)
        {
            throw new SpecificationCancelledException(operation, ex);
        }
    }

    ResourceEntry EntryFor<T>(Func<T> factory)
        where T : class, ISpecificationResource
    {
        lock (_lock)
        {
            if (_entries.TryGetValue(typeof(T), out var entry)) return entry;
            ArgumentNullException.ThrowIfNull(factory);
            entry = CreateEntry(typeof(T), factory(), owned: true);
            _entries.Add(typeof(T), entry);
            return entry;
        }
    }

    ResourceEntry CreateEntry(Type type, ISpecificationResource resource, bool owned)
    {
        var options = type.GetCustomAttribute<SpecificationResourceAttribute>() ?? new SpecificationResourceAttribute();
        if (options.MaxConcurrency < 0) throw new InvalidOperationException($"The shared resource '{type.FullName}' has a negative MaxConcurrency.");
        if (options.StopTimeoutSeconds <= 0) throw new InvalidOperationException($"The shared resource '{type.FullName}' needs a positive StopTimeoutSeconds.");

        var key = options.Key ?? type.FullName ?? type.Name;
        if (!_groups.TryGetValue(key, out var group))
        {
            group = new(key, options.MaxConcurrency);
            _groups.Add(key, group);
        }
        else if (group.MaxConcurrency != options.MaxConcurrency)
        {
            throw new InvalidOperationException(
                $"The shared resource '{type.FullName}' has MaxConcurrency {options.MaxConcurrency}, but its group '{key}' already has {group.MaxConcurrency}.");
        }

        return new(resource, owned, group, TimeSpan.FromSeconds(options.StopTimeoutSeconds));
    }

    async Task Start(ResourceEntry entry)
    {
        CancellationToken ending;
        var registerStop = false;
        lock (_lock)
        {
            ending = _ending.Token;
            if (!_stopRegistered) _stopRegistered = registerStop = true;
        }

        if (registerStop) _registerSessionEnd(Stop);

        try
        {
            await entry.Resource.Start(ending);
        }
        catch (Exception startFailure)
        {
            try
            {
                await StopResource(entry);
            }
            catch (Exception cleanupFailure)
            {
                lock (_lock) _cleanupFailures.Add(cleanupFailure);
            }

            ExceptionDispatchInfo.Throw(startFailure);
        }

        lock (_lock) _started.Add(entry);
    }
}
