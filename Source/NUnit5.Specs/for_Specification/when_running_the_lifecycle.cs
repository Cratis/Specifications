// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.for_Specification.given;

namespace Cratis.Specifications.for_Specification;

public class when_running_the_lifecycle : Specification
{
    a_lifecycle_probe probe;
    List<string> calls_before_destroy;

    async Task Because()
    {
        probe = new a_lifecycle_probe();
        await probe.InitializeAsync();
        calls_before_destroy = [.. probe.Calls];
        await probe.DisposeAsync();
    }

    [Test] public void should_establish_before_because() => calls_before_destroy.ShouldEqual(["Establish", "Because"]);
    [Test] public void should_not_destroy_before_disposing() => calls_before_destroy.ShouldNotContain("Destroy");
    [Test] public void should_destroy_exactly_once_after_because() => probe.Calls.ShouldEqual(["Establish", "Because", "Destroy"]);
}
