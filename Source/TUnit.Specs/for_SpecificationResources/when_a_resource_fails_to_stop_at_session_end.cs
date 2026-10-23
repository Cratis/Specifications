// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.Resources;

namespace Cratis.Specifications.for_SpecificationResources;

// Ends the session through the session hook the package compiles into the test project, the one TUnit runs after the session.
[NotInParallel]
public class when_a_resource_fails_to_stop_at_session_end : Specification
{
    ResourceLog log;
    Exception thrown;

    async Task Because()
    {
        log = await ResourceSessionCase.StartResourceThatFailsToStop();
        thrown = await Catch.Exception(() => CratisSpecificationsSession.End());
    }

    [Test] public void should_stop_the_resource() => log.Disposals.ShouldEqual(1);
    [Test] public void should_surface_the_stop_failure_to_tunit() => thrown.Message.ShouldContain(ResourceSessionCase.StopFailureMessage);
}
