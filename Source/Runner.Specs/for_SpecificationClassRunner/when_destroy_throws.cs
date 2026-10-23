// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_SpecificationClassRunner;

public class when_destroy_throws : given.a_probe_run<failing_destroy>
{
    void should_fail_the_facts_that_passed_with_the_exception_of_destroy() => results[0].Exception.Message.ShouldEqual("Destroy failed");
}
