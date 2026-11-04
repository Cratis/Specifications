// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace Cratis.Specifications;

public class when_readable_names_are_not_enabled : Specification
{
    [Fact] void should_not_opt_in_implicitly() => GetType().Assembly.IsDefined(typeof(ReadableSpecificationNamesAttribute)).ShouldBeFalse();
    [Fact] void should_not_generate_a_lifecycle_override() => GetType().Assembly.IsDefined(typeof(PerFactSpecificationLifecycleAttribute)).ShouldBeFalse();
}
