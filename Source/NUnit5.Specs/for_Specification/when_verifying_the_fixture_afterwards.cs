// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_Specification;

#if NUNIT5
[DependsOnFixture(typeof(when_running_a_fixture_with_lifecycle_methods))]
#else
[Order(2)]
#endif
public class when_verifying_the_fixture_afterwards : Specification
{
    [Test] public void should_have_destroyed_once_after_the_last_fact() =>
        given.a_context_logging_its_lifecycle.Calls.ToArray().ShouldEqual(
            "base.Establish", "derived.Establish", "base.Because", "derived.Because", "base.Destroy", "derived.Destroy");
}
