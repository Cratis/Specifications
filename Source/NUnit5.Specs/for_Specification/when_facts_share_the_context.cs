// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_Specification;

public class when_facts_share_the_context : Specification
{
    static int establish_count;
    static int because_count;

    object context;

    void Establish()
    {
        establish_count++;
        context = new object();
    }

    void Because() => because_count++;

    [Test] public void should_have_established_once_for_all_facts() => establish_count.ShouldEqual(1);
    [Test] public void should_have_run_because_once_for_all_facts() => because_count.ShouldEqual(1);
    [Test] public void should_have_a_context_from_establish() => context.ShouldNotBeNull();
    [Test] public void should_see_the_same_context_in_every_fact() => context.ShouldBeSame(context);
}
