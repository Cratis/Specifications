// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_SpecificationClassRunner;

public class when_running_an_asynchronous_specification : given.a_probe_run<asynchronous_specification>
{
    void should_pass_every_fact() => results.ShouldEachConformTo(_ => _.Outcome == FactOutcome.Passed);

    void should_await_the_lifecycle_in_order() =>
        calls.ShouldEqual("Establish base", "Establish", "Because base", "Because", "fact", "fact", "Destroy base", "Destroy", "DisposeAsync");
}
