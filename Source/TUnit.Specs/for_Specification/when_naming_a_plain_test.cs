// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_Specification;

public class when_naming_a_plain_test
{
    [Test] public void should_keep_the_framework_display_name() =>
        TUnit.Core.TestContext.Current.Metadata.DisplayName.ShouldEqual(nameof(should_keep_the_framework_display_name));
}
