// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit.Sdk;

namespace Cratis.Specifications.for_Should;

public class when_asserting_unsuccessfully : Specification
{
    static readonly int[] _numbers = [1, 2];
    Exception _greater_than;
    Exception _less_than_or_equal;
    Exception _collection;
    Exception _contain_only;

    void Because()
    {
        _greater_than = Catch.Exception(() => 1.ShouldBeGreaterThan(2));
        _less_than_or_equal = Catch.Exception(() => 3.ShouldBeLessThanOrEqual(2));
        _collection = Catch.Exception(() => _numbers.ShouldEqual(2, 1));
        _contain_only = Catch.Exception(() => _numbers.ShouldContainOnly(1));
    }

    [Fact] void should_fail_comparison_with_a_true_exception() => _greater_than.ShouldBeOfExactType<TrueException>();
    [Fact] void should_keep_the_comparison_message() => _greater_than.Message.ShouldContain("1 should be greater than 2");
    [Fact] void should_keep_the_less_than_or_equal_message() => _less_than_or_equal.Message.ShouldContain("3 should be less than or equal to 2");
    [Fact] void should_fail_sequence_equality_with_a_true_exception() => _collection.ShouldBeOfExactType<TrueException>();
    [Fact] void should_keep_the_sequence_equality_message() => _collection.Message.ShouldContain("is not equal to");
    [Fact] void should_fail_contain_only_with_the_original_message() => _contain_only.Message.ShouldContain("does not only contain '1'");
}
