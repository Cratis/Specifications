// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_ShouldCollectionExtensions;

public class when_asserting_a_single_item : Specification
{
    Exception _single;
    Exception _two;

    void Because()
    {
        _single = Catch.Exception(() => new[] { 1 }.ShouldContainSingleItem());
        _two = Catch.Exception(() => new[] { 1, 2 }.ShouldContainSingleItem());
    }

    [Test] public void should_pass_for_one_item() => _single.ShouldBeNull();
    [Test] public void should_fail_for_two_items() => _two.ShouldNotBeNull();
}
