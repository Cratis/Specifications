// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_SpecificationClassRunner;

public class when_because_throws : given.a_probe_run<failing_because>
{
    void should_fail_every_fact() => results.ShouldEachConformTo(_ => _.Outcome == FactOutcome.Failed);

    void should_fail_every_fact_with_the_exception_of_because() => results.ShouldEachConformTo(_ => ReferenceEquals(_.Exception, failing_because.Failure));

    void should_not_run_any_fact() => calls.ShouldNotContain("fact");

    void should_still_destroy_once() => calls.ShouldEqual("Establish base", "Because base", "Destroy base");
}
