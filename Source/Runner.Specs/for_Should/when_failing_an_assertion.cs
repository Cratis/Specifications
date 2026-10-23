// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_Should;

public class when_failing_an_assertion : Specification
{
    static readonly int[] _numbers = [1, 2, 3];
    Exception _equality;
    Exception _ordered_params;

    void Because()
    {
        _equality = Catch.Exception(() => 1.ShouldEqual(2));
        _ordered_params = Catch.Exception(() => _numbers.ShouldEqual(3, 2, 1));
    }

    void should_fail_with_a_specification_assertion_exception() => _equality.ShouldBeOfExactType<SpecificationAssertionException>();

    void should_describe_the_expected_and_actual_values() => _equality.Message.ShouldEqual($"Expected: 2{Environment.NewLine}Actual:   1");

    void should_compare_params_in_order() => _ordered_params.ShouldBeOfExactType<SpecificationAssertionException>();
}
