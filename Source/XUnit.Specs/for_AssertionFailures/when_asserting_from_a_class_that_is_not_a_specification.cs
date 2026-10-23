// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit.Sdk;

namespace Cratis.Specifications.for_AssertionFailures;

public class when_asserting_from_a_class_that_is_not_a_specification : Specification
{
    Exception _result;

    void Because() => _result = Catch.Exception(PlainAsserter.Fail);

    [Fact] void should_fail_with_an_xunit_exception() => _result.ShouldBeOfExactType<XunitException>();

    static class PlainAsserter
    {
        public static void Fail() => 1.ShouldEqual(2);
    }
}
