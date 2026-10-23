// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_Should;

public class when_asserting_successfully : Specification
{
    static readonly int[] _numbers = [1, 2];
    Exception _exception;

    void Because() => _exception = Catch.Exception(() =>
    {
        2.ShouldEqual(2);
        3.ShouldBeGreaterThan(2);
        2.ShouldBeLessThanOrEqual(2);
        _numbers.ShouldEqual(1, 2);
        _numbers.ShouldContainOnly(2, 1);
        _numbers.ShouldContain(2);
        "hello".ShouldContain("ell");
    });

    [Test] public void should_not_fail() => _exception.ShouldBeNull();
}
