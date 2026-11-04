// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_SpecificationTestFramework;

public class when_verifying_async_disposal_afterwards : Specification
{
    [Fact] void should_dispose_once() => when_running_facts_on_an_async_disposable_specification.dispose_count.ShouldEqual(1);
    [Fact] void should_destroy_once() => when_running_facts_on_an_async_disposable_specification.destroy_count.ShouldEqual(1);
}
