// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.for_SpecificationMethods.probes;

namespace Cratis.Specifications.for_SpecificationMethods;

public class when_an_async_lifecycle_method_throws : Specification
{
    probe_with_async_throwing_because _probe;
    Exception _exception;

    void Establish() => _probe = new();

    async Task Because()
    {
        _exception = await Catch.Exception(() => SpecificationMethods<probe_with_async_throwing_because, probe_root>.Because(_probe));
    }

    [Fact] void should_fail_with_the_original_exception_type() => _exception.ShouldBeOfExactType<InvalidOperationException>();
    [Fact] void should_keep_the_original_message() => _exception.Message.ShouldEqual("deliberate Because failure");
}
