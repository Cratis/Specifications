// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_Should;

public class when_asserting_unsuccessfully : Specification
{
    static readonly int[] _numbers = [1, 2];

    // NUnit records an assertion failure on the current test result even when the exception is caught,
    // so the failing assertions are made inside the facts, where the expected failure is itself the assertion.
    [Test] public void should_fail_equality_with_an_assertion_exception() => Assertion.ShouldFail(() => 1.ShouldEqual(2));
    [Test] public void should_fail_comparison_with_an_assertion_exception() => Assertion.ShouldFail(() => 1.ShouldBeGreaterThan(2));
    [Test] public void should_fail_equality_on_different_elements_with_an_assertion_exception() => Assertion.ShouldFail(() => _numbers.ShouldEqual(1, 3));
#if NUNIT5
    [Test] public void should_fail_sequence_equality_on_a_different_order_with_an_assertion_exception() => Assertion.ShouldFail(() => _numbers.ShouldEqual(2, 1));
#endif
    [Test] public void should_fail_contain_only_with_an_assertion_exception() => Assertion.ShouldFail(() => _numbers.ShouldContainOnly(1));
}
