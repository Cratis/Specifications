// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_ShouldEqualityExtensions;

public class when_asserting_null_on_a_value : Specification
{
    Exception _shouldBeNull;
    Exception _shouldNotBeNull;

    void Because()
    {
        _shouldBeNull = Catch.Exception(() => new object().ShouldBeNull());
        _shouldNotBeNull = Catch.Exception(() => ((object)null).ShouldNotBeNull());
    }

    [Test] public void should_fail_when_expecting_null_for_an_instance() => _shouldBeNull.ShouldBeOfExactType<SpecificationAssertionException>();
    [Test] public void should_fail_when_expecting_an_instance_for_null() => _shouldNotBeNull.ShouldBeOfExactType<SpecificationAssertionException>();
}
