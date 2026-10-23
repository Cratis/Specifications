// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_ShouldEqualityExtensions;

public class when_asserting_sameness : Specification
{
    Exception _sameInstance;
    Exception _differentInstances;

    void Because()
    {
        var instance = new object();
        _sameInstance = Catch.Exception(() => instance.ShouldBeSame(instance));
        _differentInstances = Catch.Exception(() => new object().ShouldBeSame(new object()));
    }

    [Test] public void should_pass_for_the_same_instance() => _sameInstance.ShouldBeNull();
    [Test] public void should_fail_for_different_instances() => _differentInstances.ShouldNotBeNull();
}
