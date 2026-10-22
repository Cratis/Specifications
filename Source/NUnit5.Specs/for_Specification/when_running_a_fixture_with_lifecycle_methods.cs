// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_Specification;

[Order(1)]
public class when_running_a_fixture_with_lifecycle_methods : given.a_context_logging_its_lifecycle
{
    void Establish() => Calls.Add("derived.Establish");

    void Because() => Calls.Add("derived.Because");

    void Destroy() => Calls.Add("derived.Destroy");

    [Test] public void should_run_establish_once() => Calls.Count(_ => _ == "derived.Establish").ShouldEqual(1);
    [Test] public void should_run_because_once() => Calls.Count(_ => _ == "derived.Because").ShouldEqual(1);
    [Test] public void should_run_the_lifecycle_of_the_base_class_before_the_derived_class() =>
        Calls.ToArray().ShouldEqual("base.Establish", "derived.Establish", "base.Because", "derived.Because");
    [Test] public void should_not_destroy_while_facts_are_running() => Calls.Any(_ => _.EndsWith("Destroy")).ShouldBeFalse();
}
