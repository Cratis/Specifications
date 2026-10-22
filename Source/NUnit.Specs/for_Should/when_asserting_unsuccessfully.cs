// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_Should;

public class when_asserting_unsuccessfully : Specification
{
    static readonly int[] _numbers = [1, 2];
    Exception _equal;
    Exception _greater_than;
    Exception _collection;
    Exception _contain_only;

    void Because()
    {
        _equal = Catch.Exception(() => 1.ShouldEqual(2));
        _greater_than = Catch.Exception(() => 1.ShouldBeGreaterThan(2));
        _collection = Catch.Exception(() => _numbers.ShouldEqual(2, 1));
        _contain_only = Catch.Exception(() => _numbers.ShouldContainOnly(1));
    }

    [Test] public void should_fail_equality_with_an_assertion_exception() => _equal.ShouldBeOfExactType<AssertionException>();
    [Test] public void should_fail_comparison_with_an_assertion_exception() => _greater_than.ShouldBeOfExactType<AssertionException>();
    [Test] public void should_fail_sequence_equality_with_an_assertion_exception() => _collection.ShouldBeOfExactType<AssertionException>();
    [Test] public void should_fail_contain_only_with_an_assertion_exception() => _contain_only.ShouldBeOfExactType<AssertionException>();
}
