// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_ShouldTypeExtensions;

public class when_asserting_types : Specification
{
    Exception _exactType;
    Exception _wrongExactType;
    Exception _assignable;
    Exception _notAssignable;

    void Because()
    {
        object value = "text";
        _exactType = Catch.Exception(() => value.ShouldBeOfExactType<string>());
        _wrongExactType = Catch.Exception(() => value.ShouldBeOfExactType<int>());
        _assignable = Catch.Exception(() => value.ShouldBeAssignableFrom<IEnumerable<char>>());
        _notAssignable = Catch.Exception(() => value.ShouldBeAssignableFrom<IDisposable>());
    }

    [Test] public void should_pass_for_the_exact_type() => _exactType.ShouldBeNull();
    [Test] public void should_fail_for_another_type() => _wrongExactType.ShouldNotBeNull();
    [Test] public void should_pass_for_an_assignable_type() => _assignable.ShouldBeNull();
    [Test] public void should_fail_for_a_type_that_is_not_assignable() => _notAssignable.ShouldNotBeNull();
}
