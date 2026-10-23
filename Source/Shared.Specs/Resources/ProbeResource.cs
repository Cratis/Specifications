// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Resources;

/// <summary>
/// A shared resource that records what is done to it and behaves as its <see cref="ProbeBehavior"/> says.
/// </summary>
internal abstract class ProbeResource : ISpecificationResource<ProbeLease>
{
    readonly ResourceLog _log;
    readonly ProbeBehavior _behavior;

    protected ProbeResource(string name, ResourceLog log, ProbeBehavior behavior)
    {
        Name = name;
        _log = log;
        _behavior = behavior ?? new();
        log.ResourceCreated();
    }

    public string Name { get; }

    public async ValueTask Start(CancellationToken cancellationToken)
    {
        _log.ResourceStarted();
        await _behavior.StartGate;
        if (_behavior.StartFailure is not null) throw _behavior.StartFailure;
    }

    public ValueTask<ProbeLease> Lease(SpecificationLeaseContext context, CancellationToken cancellationToken)
    {
        if (_behavior.LeaseFailure is not null) return ValueTask.FromException<ProbeLease>(_behavior.LeaseFailure);
        _log.Add($"lease {Name}");
        _log.LeaseTaken();
#pragma warning disable CA2000 // The lease is handed to the registry, which disposes it when the scope ends.
        return ValueTask.FromResult(new ProbeLease(Name, context, _log, _behavior.ReleaseFailure));
#pragma warning restore CA2000
    }

    public async ValueTask DisposeAsync()
    {
        _log.ResourceDisposed();
        _log.Add($"stop {Name}");
        if (_behavior.StopHangs) await Task.Delay(Timeout.Infinite);
        if (_behavior.StopFailure is not null) throw _behavior.StopFailure;
    }
}
