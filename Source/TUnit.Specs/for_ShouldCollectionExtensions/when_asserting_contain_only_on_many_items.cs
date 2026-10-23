// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_ShouldCollectionExtensions;

public class when_asserting_contain_only_on_many_items : Specification
{
    Exception _failure;

    void Because()
    {
        var items = Enumerable.Range(0, 20).ToArray();
        var expected = Enumerable.Range(0, 19).Append(99).ToArray();
        _failure = Catch.Exception(() => items.ShouldContainOnly(expected));
    }

    [Test]
    public void should_describe_both_collections_in_the_failure() =>
        _failure.Message.ShouldEqual("Collection '0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19' does not only contain '0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,99'");
}
