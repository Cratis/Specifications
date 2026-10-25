// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_SpecificationClassRunner;

public class when_destroy_throws : given.a_probe_run<failing_destroy>
{
    void should_keep_the_passing_fact() => results[0].Outcome.ShouldEqual(FactOutcome.Passed);
    void should_not_attach_the_teardown_error_to_the_fact() => results[0].Exception.ShouldBeNull();
    void should_report_a_separate_teardown_failure() => teardown_failure.Value.Exception.Message.ShouldEqual("Destroy failed");
}
