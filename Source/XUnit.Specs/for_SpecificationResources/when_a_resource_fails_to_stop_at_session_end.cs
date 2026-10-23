// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.Resources;

namespace Cratis.Specifications.for_SpecificationResources;

// Ends the session through the hook the xUnit assembly runner awaits in its cleanup aggregator.
public class when_a_resource_fails_to_stop_at_session_end : Specification
{
    ResourceLog log;
    Exception thrown;

    async Task Because()
    {
        log = await ResourceSessionCase.StartResourceThatFailsToStop();
        thrown = await Catch.Exception(SpecificationSessionHook.End);
    }

    [Fact] void should_stop_the_resource() => log.Disposals.ShouldEqual(1);
    [Fact] void should_surface_the_stop_failure_to_the_assembly_cleanup() => thrown.Message.ShouldContain(ResourceSessionCase.StopFailureMessage);
}
