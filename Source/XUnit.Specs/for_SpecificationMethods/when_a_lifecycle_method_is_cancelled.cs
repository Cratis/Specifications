// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.for_SpecificationMethods.probes;

namespace Cratis.Specifications.for_SpecificationMethods;

public class when_a_lifecycle_method_is_cancelled : Specification
{
    probe_with_cancelling_establish _probe;
    Exception _exception;

    void Establish() => _probe = new();

    async Task Because()
    {
        _exception = await Catch.Exception(() => SpecificationMethods<probe_with_cancelling_establish, probe_root>.Establish(_probe));
    }

    [Fact] void should_fail_with_a_specification_cancelled_exception() => _exception.ShouldBeOfExactType<SpecificationCancelledException>();
    [Fact] void should_keep_the_cancellation_as_inner_exception() => (_exception.InnerException is OperationCanceledException).ShouldBeTrue();
    [Fact] void should_name_the_phase_in_the_message() => _exception.Message.ShouldContain("Establish()");
}
