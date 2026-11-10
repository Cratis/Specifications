// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.Resources;

namespace Cratis.Specifications.for_ScopeLeases;

public class when_registering_leases_concurrently : Specification
{
    SpecificationScope scope;
    ScopeLeases holder;
    ResourceGroup group;
    TrackedLease[] leases;

    void Establish()
    {
        scope = new(new object());
        holder = ScopeLeases.For(scope);
        group = new("unlimited", 0);
        leases = [.. Enumerable.Range(0, 32).Select(_ => new TrackedLease())];
    }

    async Task Because()
    {
        await Task.WhenAll(leases.Select(lease => Task.Run(() => holder.Add(group, lease))));
        await ScopeLeases.Release(scope);
    }

    [Fact]
    void should_release_every_registered_lease_once() => leases.ShouldEachConformTo(_ => _.Releases == 1);
}
