// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NUnit.Framework.Internal;

[assembly: Cratis.Specifications.ReadableSpecificationNames]

namespace Cratis.Specifications.for_Specification;

public class when_using_readable_names : Specification
{
    [Test] public void should_display_the_specification_hierarchy() =>
        TestContext.CurrentContext.Test.Name.ShouldEqual("Specification › when using readable names › should display the specification hierarchy");

    [Test] public void should_leave_an_assembly_without_the_attribute_unchanged()
    {
        var suite = new TestSuite(typeof(Specification));
        var original = suite.Name;
        new SpecificationDisplayNameAttribute().ApplyToTest(suite);
        suite.Name.ShouldEqual(original);
    }
}
