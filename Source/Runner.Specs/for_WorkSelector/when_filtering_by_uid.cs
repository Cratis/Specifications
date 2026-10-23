// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Requests;

namespace Cratis.Specifications.Runner.for_WorkSelector;

public class when_filtering_by_uid : Specification
{
    IReadOnlyList<SpecificationWork> selected;

    void Because() => selected = WorkSelector.Select(
        given.the_probes.All,
        new TestNodeUidListFilter([new TestNodeUid(typeof(failing_because).FullName + ".should_fail_second"), new TestNodeUid(typeof(synchronous_specification).FullName)]));

    void should_select_only_the_classes_with_selected_facts() => selected.Select(_ => _.Specification.Uid).ShouldContainOnly(typeof(failing_because).FullName, typeof(synchronous_specification).FullName);

    void should_select_only_the_selected_fact() => selected.Single(_ => _.Specification.Uid == typeof(failing_because).FullName).Facts.Select(_ => _.MethodName).ShouldContainOnly("should_fail_second");

    void should_select_every_fact_of_a_selected_class() => selected.Single(_ => _.Specification.Uid == typeof(synchronous_specification).FullName).Facts.Count.ShouldEqual(3);
}
