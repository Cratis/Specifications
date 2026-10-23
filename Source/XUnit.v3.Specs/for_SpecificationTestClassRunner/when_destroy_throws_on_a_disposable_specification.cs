// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.for_SpecificationTestClassRunner.probes;

namespace Cratis.Specifications.for_SpecificationTestClassRunner;

public class when_destroy_throws_on_a_disposable_specification : given.a_class_runner_harness
{
    void Establish() => disposable_specification_with_throwing_destroy.dispose_count = 0;
    Task Because() => RunClass(typeof(disposable_specification_with_throwing_destroy));
    [Fact] void should_still_dispose_once() => disposable_specification_with_throwing_destroy.dispose_count.ShouldEqual(1);
}
