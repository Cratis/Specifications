// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_SpecificationClassRunner;

public class when_a_class_declares_both_establish_and_given : given.a_probe_run<establish_and_given>
{
    void should_fail_every_fact_with_a_discovery_error() => results.ShouldEachConformTo(_ => _.Exception is SpecificationDiscoveryException);

    void should_name_the_conflict() => results[0].Exception.Message.ShouldContain("both Establish and Given");

    void should_not_run_anything() => calls.ShouldBeEmpty();
}
