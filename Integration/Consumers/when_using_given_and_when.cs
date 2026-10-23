// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications;

namespace Consumer.for_Setup;

public class when_using_given_and_when : Specification
{
    int _value;
    void Given() => _value = 41;
    void When() => _value++;

#if XUNIT
    [Xunit.Fact]
#elif NUNIT
    [NUnit.Framework.Test]
#elif TUNIT
    [global::TUnit.Core.Test]
#else
    [Fact]
#endif
    public void should_run_the_lifecycle()
    {
        _value.ShouldEqual(42);
#if NUNIT
        NUnit.Framework.TestContext.CurrentContext.Test.Name.ShouldEqual("Setup › when using given and when › should run the lifecycle");
#endif
    }
}
