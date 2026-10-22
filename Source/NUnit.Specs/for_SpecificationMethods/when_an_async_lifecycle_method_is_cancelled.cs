// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.for_SpecificationMethods.probes;

namespace Cratis.Specifications.for_SpecificationMethods;

public class when_an_async_lifecycle_method_is_cancelled : Specification
{
    probe_with_async_cancelling_because _probe;
    Exception _exception;

    void Establish() => _probe = new();

    async Task Because()
    {
        _exception = await Catch.Exception(() => SpecificationMethods<probe_with_async_cancelling_because, probe_root>.Because(_probe));
    }

    [Test] public void should_fail_with_a_specification_cancelled_exception() => _exception.ShouldBeOfExactType<SpecificationCancelledException>();
    [Test] public void should_keep_the_cancellation_as_inner_exception() => (_exception.InnerException is OperationCanceledException).ShouldBeTrue();
    [Test] public void should_name_the_phase_in_the_message() => _exception.Message.ShouldContain("Because()");
}
