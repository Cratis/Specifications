// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications;
using Xunit;

#pragma warning disable CRSPEC0008 // The facts mutate state on purpose to prove each one gets a fresh instance.

namespace Consumer.for_Lifecycle;

public class when_opting_out_of_shared_lifecycle : Specification
{
    int _value;

    void Establish() => _value = 42;

    [Fact]
    public void should_start_the_first_fact_with_fresh_state()
    {
        _value.ShouldEqual(42);
        _value++;
    }

    [Fact]
    public void should_start_the_second_fact_with_fresh_state()
    {
        _value.ShouldEqual(42);
        _value++;
    }

    [Fact]
    public void should_not_register_the_shared_framework() =>
        typeof(when_opting_out_of_shared_lifecycle).Assembly.GetCustomAttributes(typeof(TestFrameworkAttribute), false).ShouldBeEmpty();

    [Fact]
    public void should_not_enable_readable_names() =>
        typeof(when_opting_out_of_shared_lifecycle).Assembly.GetCustomAttributes(typeof(ReadableSpecificationNamesAttribute), false).ShouldBeEmpty();
}
