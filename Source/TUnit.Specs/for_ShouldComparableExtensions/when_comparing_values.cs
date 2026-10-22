// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_ShouldComparableExtensions;

public class when_comparing_values : Specification
{
    Exception _greaterThan;
    Exception _notGreaterThan;
    Exception _lessThanOrEqual;
    Exception _inRange;
    Exception _outOfRange;

    void Because()
    {
        _greaterThan = Catch.Exception(() => 5.ShouldBeGreaterThan(3));
        _notGreaterThan = Catch.Exception(() => 3.ShouldBeGreaterThan(5));
        _lessThanOrEqual = Catch.Exception(() => 3.ShouldBeLessThanOrEqual(3));
        _inRange = Catch.Exception(() => 5.ShouldBeInRange(1, 10));
        _outOfRange = Catch.Exception(() => 11.ShouldBeInRange(1, 10));
    }

    [Test] public void should_pass_when_greater() => _greaterThan.ShouldBeNull();
    [Test] public void should_fail_when_not_greater() => _notGreaterThan.ShouldNotBeNull();
    [Test] public void should_pass_when_less_than_or_equal() => _lessThanOrEqual.ShouldBeNull();
    [Test] public void should_pass_when_in_range() => _inRange.ShouldBeNull();
    [Test] public void should_fail_when_out_of_range() => _outOfRange.ShouldNotBeNull();
}
