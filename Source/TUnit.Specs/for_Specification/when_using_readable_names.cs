// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

[assembly: Cratis.Specifications.ReadableSpecificationNames]
[assembly: Cratis.Specifications.SpecificationDisplayName]

namespace Cratis.Specifications.for_Specification;

public class when_using_readable_names : Specification
{
    [Test] public void should_display_the_specification_hierarchy() =>
        TUnit.Core.TestContext.Current.Metadata.DisplayName.ShouldEqual("Specification › when using readable names › should display the specification hierarchy");
}
