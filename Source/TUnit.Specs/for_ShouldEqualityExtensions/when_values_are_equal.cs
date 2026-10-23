// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_ShouldEqualityExtensions;

public class when_values_are_equal : Specification
{
    Exception _result;

    void Because() => _result = Catch.Exception(() => 42.ShouldEqual(42));

    [Test] public void should_not_fail() => _result.ShouldBeNull();
}
