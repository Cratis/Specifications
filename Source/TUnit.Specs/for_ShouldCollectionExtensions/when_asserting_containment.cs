// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_ShouldCollectionExtensions;

public class when_asserting_containment : Specification
{
    Exception _contained;
    Exception _missing;
    Exception _notContainedAbsent;
    Exception _notContainedPresent;

    void Because()
    {
        int[] items = [1, 2, 3];
        _contained = Catch.Exception(() => items.ShouldContain(2));
        _missing = Catch.Exception(() => items.ShouldContain(4));
        _notContainedAbsent = Catch.Exception(() => items.ShouldNotContain(4));
        _notContainedPresent = Catch.Exception(() => items.ShouldNotContain(2));
    }

    [Test] public void should_pass_when_the_item_is_contained() => _contained.ShouldBeNull();
    [Test] public void should_fail_when_the_item_is_missing() => _missing.ShouldNotBeNull();
    [Test] public void should_pass_when_the_item_is_absent_and_not_expected() => _notContainedAbsent.ShouldBeNull();
    [Test] public void should_fail_when_the_item_is_present_and_not_expected() => _notContainedPresent.ShouldNotBeNull();
}
