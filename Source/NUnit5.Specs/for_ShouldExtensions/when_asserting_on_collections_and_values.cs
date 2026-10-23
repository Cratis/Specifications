// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_ShouldExtensions;

// Failing assertions are exercised inside the facts: NUnit records (rather than throws) assertion failures raised during one-time setup.
public class when_asserting_on_collections_and_values : Specification
{
    int[] numbers;

    void Establish() => numbers = [1, 2, 3];

    [Test] public void should_find_an_element_matching_the_predicate() => numbers.ShouldContain(_ => _ == 2);
    [Test] public void should_conform_when_all_elements_match() => numbers.ShouldEachConformTo(_ => _ > 0);
    [Test] public void should_be_equal_for_equal_values() => 42.ShouldEqual(42);
    [Test] public void should_fail_equality_for_different_values() => Assertion.ShouldFail(() => 42.ShouldEqual(43));
    [Test] public void should_fail_when_no_element_matches() => Assertion.ShouldFail(() => numbers.ShouldContain(_ => _ > 5));
    [Test] public void should_fail_when_not_all_elements_match() => Assertion.ShouldFail(() => numbers.ShouldEachConformTo(_ => _ > 1));
}
