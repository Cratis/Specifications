// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_ShouldCollectionExtensions;

public class when_collections_are_equal : Specification
{
    Exception _result;

    void Because() => _result = Catch.Exception(() => new[] { 1, 2, 3 }.ShouldEqual(1, 2, 3));

    [Test] public void should_not_fail() => _result.ShouldBeNull();
}
