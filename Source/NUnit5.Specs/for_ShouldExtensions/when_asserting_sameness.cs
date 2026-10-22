// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_ShouldExtensions;

// Failing assertions are exercised inside the facts: NUnit records (rather than throws) assertion failures raised during one-time setup.
public class when_asserting_sameness : Specification
{
    object instance;
    object same;
    object other;

    void Establish()
    {
        instance = new object();
        same = instance;
        other = new object();
    }

    [Test] public void should_pass_for_the_same_instance() => instance.ShouldBeSame(same);
    [Test] public void should_pass_not_same_for_a_different_instance() => instance.ShouldNotBeSame(other);
    [Test] public void should_fail_for_a_different_instance() => Assert.Throws<AssertionException>(() => instance.ShouldBeSame(other));
    [Test] public void should_fail_not_same_for_the_same_instance() => Assert.Throws<AssertionException>(() => instance.ShouldNotBeSame(same));
}
