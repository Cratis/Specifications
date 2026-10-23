// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_Specification;

public class when_disposing_a_specification_whose_destroy_throws : Specification
{
    given.a_disposable_specification_with_a_throwing_destroy _specification;
    Exception _exception;

    void Establish() => _specification = new();

    async Task Because()
    {
        try
        {
            await _specification.DisposeAsync();
        }
        catch (Exception ex)
        {
            _exception = ex;
        }
    }

    [Fact] void should_surface_the_exception_from_destroy() => _exception.ShouldBeOfExactType<InvalidOperationException>();

    [Fact] void should_still_dispose_the_specification() => _specification.Disposed.ShouldBeTrue();
}
