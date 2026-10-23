// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_Reporting;

public class when_a_fact_fails_an_assertion : Specification
{
    string result;
    SpecificationAssertionException exception;

    void Because()
    {
        exception = Catch.Exception<SpecificationAssertionException>(() => 2.ShouldEqual(1));
        result = rendered_tree.Render(new(OutputLevel.Normal, false, false, false, null, []));
    }

    void should_carry_the_expected_value() => exception.Expected.ShouldEqual("1");

    void should_carry_the_actual_value() => exception.Actual.ShouldEqual("2");

    void should_show_expected_and_actual() => result.ShouldContain("Expected: 1");

    void should_show_the_location() => result.ShouldContain("at /src/Account.cs:41");
}
