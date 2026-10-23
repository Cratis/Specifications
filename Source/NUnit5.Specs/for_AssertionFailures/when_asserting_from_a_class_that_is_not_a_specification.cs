// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_AssertionFailures;

public class when_asserting_from_a_class_that_is_not_a_specification : Specification
{
    // NUnit records an assertion failure on the current test result even when the exception is caught,
    // so the failing assertion is made inside the fact, where the expected failure is itself the assertion.
    [Test] public void should_fail_with_an_assertion_exception() => Assert.Throws<AssertionException>(PlainAsserter.Fail);

    static class PlainAsserter
    {
        public static void Fail() => 1.ShouldEqual(2);
    }
}
