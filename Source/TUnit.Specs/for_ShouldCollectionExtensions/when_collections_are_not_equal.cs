// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_ShouldCollectionExtensions;

public class when_collections_are_not_equal : Specification
{
    Exception _result;

    void Because() => _result = Catch.Exception(() => new[] { 1, 2, 3 }.ShouldEqual(1, 2, 4));

    [Test] public void should_fail() => _result.ShouldBeOfExactType<SpecificationAssertionException>();
    [Test] public void should_report_the_expected_items() => _result.Message.ShouldContain("[1, 2, 4]");
    [Test] public void should_report_the_actual_items() => _result.Message.ShouldContain("[1, 2, 3]");
}
