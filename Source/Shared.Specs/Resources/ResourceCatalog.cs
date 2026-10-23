// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Resources;

/// <summary>
/// The contract of the shared resources, run by every package's specs on that package's runtime.
/// </summary>
/// <remarks>
/// Each case works against its own <see cref="ResourceRegistry"/>, and leases inside a scope it starts and ends
/// itself, so the cases never share state with each other or with the specification running them.
/// </remarks>
internal static partial class ResourceCatalog
{
    static readonly TimeSpan _patience = TimeSpan.FromSeconds(10);
    static readonly ResourceLeaseObserver _leaseObserver = new();

    public static async Task<ResourceCatalogResults> Run()
    {
        var results = new ResourceCatalogResults();
        await StartOncePerResource(results);
        await StartNothingNeverAcquired(results);
        await CacheStartFailures(results);
        await ReleaseInReverseOrder(results);
        await LimitConcurrency(results);
        await RejectOutOfOrderLeases(results);
        await CancelWaits(results);
        await ReportStopFailures(results);
        await NeverDisposeBorrowed(results);
        await FailOnlyTheFailingLease(results);
        await RejectLeasesOutsideAScope(results);
        await RejectLeasesWithoutOptIn(results);
        return results;
    }

    static async Task StartOncePerResource(ResourceCatalogResults results)
    {
        var (registry, session) = NewRegistry();
        var log = new ResourceLog();
        var gate = new TaskCompletionSource();
        var acquirers = Enumerable.Range(0, 16)
            .Select(_ => Task.Run(() => registry.Get(() => new unlimited_resource(log, new() { StartGate = gate.Task }), default).AsTask()))
            .ToArray();
        await Task.Delay(50);
        gate.SetResult();
        var resources = await Task.WhenAll(acquirers);

        results.StartsForParallelAcquirers = log.Starts;
        results.CreatedForParallelAcquirers = log.Created;
        results.InstancesForParallelAcquirers = resources.Distinct().Count();
        results.SessionEndRegistrations = session.Registrations;
    }

    static async Task StartNothingNeverAcquired(ResourceCatalogResults results)
    {
        var (registry, session) = NewRegistry();
        var log = new ResourceLog();
        var scope = NewScope("never acquires");
        await Establish(scope, () => Task.CompletedTask);
        await End(scope);
        await registry.Stop();

        results.CreatedWhenNeverAcquired = log.Created;
        results.SessionEndRegistrationsWhenNeverAcquired = session.Registrations;
    }

    static async Task CacheStartFailures(ResourceCatalogResults results)
    {
        var (registry, session) = NewRegistry();
        var log = new ResourceLog();
        results.StartFailure = new InvalidOperationException("Start failure");
        var behavior = new ProbeBehavior { StartFailure = results.StartFailure };
        results.FirstAcquirerFailure = await Catch(() => registry.Get(() => new unlimited_resource(log, behavior), default).AsTask());
        results.SecondAcquirerFailure = await Catch(() => registry.Get(() => new unlimited_resource(log, behavior), default).AsTask());
        results.StartsAfterFailure = log.Starts;
        results.DisposalsAfterFailedStart = log.Disposals;

        results.PartialStartCleanupFailure = new InvalidOperationException("Cleanup failure");
        var failsTwice = new ProbeBehavior { StartFailure = new InvalidOperationException("Start failure"), StopFailure = results.PartialStartCleanupFailure };
        await Catch(() => registry.Get(() => new quickly_stopped_resource(log, failsTwice), default).AsTask());
        results.ReportedPartialStartCleanupFailure = await Catch(() => session.Stop().AsTask());
    }

    static async Task ReleaseInReverseOrder(ResourceCatalogResults results)
    {
        var (registry, _) = NewRegistry();
        var log = new ResourceLog();
        var scope = NewScope("releases in reverse");
        var leases = new List<ProbeLease>();
        await Establish(scope, async () =>
        {
            leases.Add(await registry.Lease<first_exclusive_resource, ProbeLease>(() => new(log), default));
            leases.Add(await registry.Lease<second_exclusive_resource, ProbeLease>(() => new(log), default));
            leases.Add(await registry.Lease<unlimited_resource, ProbeLease>(() => new(log), default));
            log.Add("established");
        });
        await End(scope);
        results.ReleaseSequence = log.Entries;
        await registry.Stop();

        results.LeaseDisplayName = leases[0].Context.DisplayName;
        results.LeaseIdsAreUnique = leases.Select(_ => _.Context.LeaseId).Distinct().Count() == leases.Count;
    }

    static async Task LimitConcurrency(ResourceCatalogResults results)
    {
        var (registry, _) = NewRegistry();
        var log = new ResourceLog();
        await Task.WhenAll(Enumerable.Range(0, 8).Select(index => Task.Run(async () =>
        {
            var scope = NewScope($"exclusive {index}");
            await Establish(scope, () => registry.Lease<first_exclusive_resource, ProbeLease>(() => new(log), default).AsTask());
            await Task.Delay(5);
            await End(scope);
        })));
        results.PeakForExclusiveGroup = log.Peak;
        await registry.Stop();

        var independent = new ResourceLog();
        var first = NewScope("first group");
        var second = NewScope("second group");
        await Establish(first, () => registry.Lease<first_exclusive_resource, ProbeLease>(() => new(independent), default).AsTask());
        results.IndependentGroupsLeasedTogether = await CompletesWithinPatience(
            Establish(second, () => registry.Lease<second_exclusive_resource, ProbeLease>(() => new(independent), default).AsTask()));
        results.PeakForIndependentGroups = independent.Peak;
        await End(first);
        await End(second);
        await registry.Stop();
    }

    static async Task RejectOutOfOrderLeases(ResourceCatalogResults results)
    {
        var (registry, _) = NewRegistry();
        var log = new ResourceLog();
        var scope = NewScope("out of order");
        results.OutOfOrderFailure = await Establish(scope, async () =>
        {
            await registry.Lease<second_exclusive_resource, ProbeLease>(() => new(log), default);
            await registry.Lease<first_exclusive_resource, ProbeLease>(() => new(log), default);
        });
        await End(scope);

        var next = NewScope("after out of order");
        results.OutOfOrderLeaseLeftGroupFree = await CompletesWithinPatience(
            Establish(next, () => registry.Lease<first_exclusive_resource, ProbeLease>(() => new(log), default).AsTask()));
        await End(next);
        await registry.Stop();
    }

    static async Task CancelWaits(ResourceCatalogResults results)
    {
        var (registry, _) = NewRegistry();
        var log = new ResourceLog();
        var holder = NewScope("holds the lease");
        var waiter = NewScope("waits for the lease");
        await Establish(holder, () => registry.Lease<first_exclusive_resource, ProbeLease>(() => new(log), default).AsTask());
        using (var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(50)))
        {
            results.CancelledWaitingForLease = await Establish(waiter, () => registry.Lease<first_exclusive_resource, ProbeLease>(() => new(log), cancellation.Token).AsTask());
        }

        await End(waiter);
        await End(holder);

        var gate = new TaskCompletionSource();
        using (var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(50)))
        {
            results.CancelledWaitingForStart = await Catch(() => registry.Get(() => new unlimited_resource(log, new() { StartGate = gate.Task }), cancellation.Token).AsTask());
        }

        gate.SetResult();
        results.StartCarriedOnAfterCancelledWait = await CompletesWithinPatience(registry.Get<unlimited_resource>(() => throw new InvalidOperationException("Created twice"), default).AsTask());
        await registry.Stop();
    }

    static async Task ReportStopFailures(ResourceCatalogResults results)
    {
        var (registry, session) = NewRegistry();
        var log = new ResourceLog();
        results.StopFailure = new InvalidOperationException("Stop failure");
        await registry.Get(() => new unlimited_resource(log, new() { StopFailure = results.StopFailure }), default);
        await registry.Get(() => new quickly_stopped_resource(log, new() { StopHangs = true }), default);
        results.ReportedStopFailures = await Catch(() => session.Stop().AsTask());
        results.StopSequence = log.Entries;
    }

    static async Task NeverDisposeBorrowed(ResourceCatalogResults results)
    {
        var (registry, _) = NewRegistry();
        var log = new ResourceLog();
#pragma warning disable CA2000 // A borrowed resource is owned by the case, and disposing it is exactly what the case asserts never happens.
        var borrowed = new unlimited_resource(log);
#pragma warning restore CA2000
        registry.Borrow(borrowed);
        results.BorrowedResourceReturned = ReferenceEquals(await registry.Get<unlimited_resource>(() => new(log), default), borrowed);
        await registry.Stop();
        results.BorrowedStarts = log.Starts;
        results.BorrowedDisposals = log.Disposals;
    }
}
