// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_SpecificationClassRunner;

public class when_a_fact_has_an_unsupported_return_type : given.a_probe_run<fact_with_an_unsupported_return_type>
{
    void should_fail_every_fact_with_a_discovery_error() => results.ShouldEachConformTo(_ => _.Exception is SpecificationDiscoveryException);

    void should_name_the_method() => results[0].Exception.Message.ShouldContain("should_return_a_value must return void, Task or ValueTask");

    void should_list_the_dropped_fact() => work.Facts.Count.ShouldEqual(2);

    void should_not_run_anything() => calls.ShouldBeEmpty();
}
