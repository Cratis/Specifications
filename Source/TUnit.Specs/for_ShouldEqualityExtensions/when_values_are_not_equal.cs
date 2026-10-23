// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_ShouldEqualityExtensions;

public class when_values_are_not_equal : Specification
{
    Exception _result;

    void Because() => _result = Catch.Exception(() => 42.ShouldEqual(43));

    [Test] public void should_fail_with_a_specification_assertion_exception() => _result.ShouldBeOfExactType<SpecificationAssertionException>();
    [Test] public void should_report_the_expected_value() => _result.Message.ShouldContain("Expected: 43");
    [Test] public void should_report_the_actual_value() => _result.Message.ShouldContain("Actual:   42");
}
