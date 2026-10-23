// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.for_SpecificationMethods.probes;

namespace Cratis.Specifications.for_SpecificationMethods;

public class when_a_lifecycle_method_throws : Specification
{
    probe_with_throwing_establish _probe;
    Exception _exception;
    Exception _thrown_synchronously;
    Task _task;

    void Establish() => _probe = new();

    async Task Because()
    {
        _thrown_synchronously = Catch.Exception(() => { _task = SpecificationMethods<probe_with_throwing_establish, probe_root>.Establish(_probe); });
        _exception = await Catch.Exception(() => _task);
    }

    [Test] public void should_not_throw_synchronously() => _thrown_synchronously.ShouldBeNull();
    [Test] public void should_fail_with_the_original_exception_type() => _exception.ShouldBeOfExactType<InvalidOperationException>();
    [Test] public void should_not_wrap_the_exception_in_a_target_invocation_exception() => (_exception is System.Reflection.TargetInvocationException).ShouldBeFalse();
    [Test] public void should_keep_the_original_message() => _exception.Message.ShouldEqual("deliberate Establish failure");
}
