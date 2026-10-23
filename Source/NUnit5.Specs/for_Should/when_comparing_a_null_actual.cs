// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_Should;

public class when_comparing_a_null_actual : Specification
{
    [Test]
    public void should_fail_greater_than_with_an_assertion_exception() =>
        Assert.Throws<AssertionException>(() => ((IComparable)null).ShouldBeGreaterThan(1));

    [Test]
    public void should_fail_greater_than_or_equal_with_an_assertion_exception() =>
        Assert.Throws<AssertionException>(() => ((IComparable)null).ShouldBeGreaterThanOrEqual(1));

    [Test]
    public void should_fail_less_than_with_an_assertion_exception() =>
        Assert.Throws<AssertionException>(() => ((IComparable)null).ShouldBeLessThan(1));

    [Test]
    public void should_fail_less_than_or_equal_with_an_assertion_exception() =>
        Assert.Throws<AssertionException>(() => ((IComparable)null).ShouldBeLessThanOrEqual(1));

    [Test]
    public void should_fail_in_range_with_an_assertion_exception() =>
        Assert.Throws<AssertionException>(() => ((string)null).ShouldBeInRange("a", "z"));

    [Test]
    public void should_fail_not_in_range_with_an_assertion_exception() =>
        Assert.Throws<AssertionException>(() => ((string)null).ShouldNotBeInRange("a", "z"));
}
