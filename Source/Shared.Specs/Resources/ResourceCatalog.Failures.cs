// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Resources;

/// <summary>
/// The lease failure cases of the <see cref="ResourceCatalog"/>, and the helpers every case uses.
/// </summary>
internal static partial class ResourceCatalog
{
    static async Task FailOnlyTheFailingLease(ResourceCatalogResults results)
    {
        var (registry, _) = NewRegistry();
        var log = new ResourceLog();
        results.LeaseFailure = new InvalidOperationException("Lease failure");
        var failing = NewScope("lease fails");
        results.FailedLeaseEstablishFailure = await Establish(failing, () => registry.Lease<first_exclusive_resource, ProbeLease>(() => new(log, new() { LeaseFailure = results.LeaseFailure }), default).AsTask());
        await failing.End();
        await registry.Stop();

        var released = new ResourceLog();
        results.ReleaseFailure = new InvalidOperationException("Release failure");
        var releasing = NewScope("release fails");
        await Establish(releasing, () => registry.Lease<first_exclusive_resource, ProbeLease>(() => new(released, new() { ReleaseFailure = results.ReleaseFailure }), default).AsTask());
        results.ScopeEndFailure = await Catch(() => releasing.End().AsTask());
        var next = NewScope("after a failed release");
        results.FailedReleaseLeftGroupFree = await CompletesWithinPatience(Establish(next, () => registry.Lease<first_exclusive_resource, ProbeLease>(() => throw new InvalidOperationException("Created twice"), default).AsTask()));
        await Catch(() => next.End().AsTask());
        await registry.Stop();

        // A failed lease never held a slot, so the next lease on the group is not kept waiting.
        var afterFailedLease = NewRegistry().Registry;
        var first = NewScope("first lease fails");
        await Establish(first, () => afterFailedLease.Lease<first_exclusive_resource, ProbeLease>(() => new(log, new() { LeaseFailure = results.LeaseFailure }), default).AsTask());
        var second = NewScope("leases after the failure");
        var secondLease = Establish(second, () => afterFailedLease.Lease<first_exclusive_resource, ProbeLease>(() => throw new InvalidOperationException("Created twice"), default).AsTask());
        results.FailedLeaseLeftGroupFree = await CompletesWithinPatience(secondLease) && await secondLease is InvalidOperationException { Message: "Lease failure" };
        await first.End();
        await second.End();
    }

    static async Task RejectLeasesOutsideAScope(ResourceCatalogResults results)
    {
        var (registry, _) = NewRegistry();
        var log = new ResourceLog();
        Task outside;
        using (ExecutionContext.SuppressFlow())
        {
            outside = Task.Run(() => registry.Lease<unlimited_resource, ProbeLease>(() => new(log), default).AsTask());
        }

        results.LeaseOutsideScopeFailure = await Catch(() => outside);
    }

    static (ResourceRegistry Registry, CatalogSession Session) NewRegistry()
    {
        var session = new CatalogSession();
        return (new ResourceRegistry(session.Register), session);
    }

    static SpecificationScope NewScope(string name) => new(new object(), name);

    static async Task<Exception> Establish(SpecificationScope scope, Func<Task> body)
    {
        try
        {
            await scope.Run(SpecificationPhase.Establish, async _ => await body());
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    static async Task<Exception> Catch(Func<Task> body)
    {
        try
        {
            await body();
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    static async Task<bool> CompletesWithinPatience(Task task) => await Task.WhenAny(task, Task.Delay(_patience)) == task;

    /// <summary>
    /// Stands in for the test session a registry registers its stop with.
    /// </summary>
    sealed class CatalogSession
    {
        public int Registrations { get; private set; }
        public Func<ValueTask> Stop { get; private set; } = () => ValueTask.CompletedTask;

        public void Register(Func<ValueTask> stop)
        {
            Registrations++;
            Stop = stop;
        }
    }
}
