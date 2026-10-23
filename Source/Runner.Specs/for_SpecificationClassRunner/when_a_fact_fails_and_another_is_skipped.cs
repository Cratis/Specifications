// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_SpecificationClassRunner;

public class when_a_fact_fails_and_another_is_skipped : given.a_probe_run<failing_fact_and_skipped_fact>
{
    void should_pass_the_passing_fact() => Result("should_pass").Outcome.ShouldEqual(FactOutcome.Passed);

    void should_fail_the_failing_fact_with_its_exception() => Result("should_fail").Exception.Message.ShouldEqual("Fact failed");

    void should_skip_the_skipped_fact_with_its_reason() => Result("should_be_skipped").SkipReason.ShouldEqual("Not yet");
}
