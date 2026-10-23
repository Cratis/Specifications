// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_Specification;

public class when_using_lifecycle_aliases : Specification
{
    int _value;
    Task Given() { _value = 41; return Task.CompletedTask; }
    ValueTask When() { _value++; return ValueTask.CompletedTask; }
    [Test] public void should_run_the_aliases() => _value.ShouldEqual(42);
}
