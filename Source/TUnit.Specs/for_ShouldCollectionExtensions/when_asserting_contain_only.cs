// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_ShouldCollectionExtensions;

public class when_asserting_contain_only : Specification
{
    Exception _sameItemsInOtherOrder;
    Exception _extraItem;

    void Because()
    {
        int[] items = [1, 2, 3];
        _sameItemsInOtherOrder = Catch.Exception(() => items.ShouldContainOnly(3, 2, 1));
        _extraItem = Catch.Exception(() => items.ShouldContainOnly(1, 2));
    }

    [Test] public void should_pass_for_the_same_items_in_any_order() => _sameItemsInOtherOrder.ShouldBeNull();
    [Test] public void should_fail_when_there_are_extra_items() => _extraItem.ShouldNotBeNull();
}
