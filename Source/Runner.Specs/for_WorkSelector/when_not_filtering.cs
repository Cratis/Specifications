// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_WorkSelector;

public class when_not_filtering : Specification
{
    IReadOnlyList<SpecificationWork> selected;

    void Because() => selected = WorkSelector.Select(given.the_probes.All, null);

    void should_select_every_class() => selected.Count.ShouldEqual(given.the_probes.All.Count);
}
