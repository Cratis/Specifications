// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// TreeNodeFilter is experimental; these specs verify the runner's --treenode-filter support against the pinned MTP API.
#pragma warning disable TPEXP

using System.Runtime.CompilerServices;
using Microsoft.Testing.Platform.Requests;

namespace Cratis.Specifications.Runner.for_WorkSelector;

public class when_filtering_by_tree_path : Specification
{
    readonly SpecificationClassDescriptor specification = given.the_probes.Get<synchronous_specification>();

    IReadOnlyList<SpecificationWork> Select(string expression) =>
        WorkSelector.Select(given.the_probes.All, CreateFilter(expression), "Probe.Assembly");

    // MTP has no public constructor; keep access to its pinned internal constructor confined to this test seam.
    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    static extern TreeNodeFilter CreateFilter(string filter);

    string PathFor(string method) => $"/Probe.Assembly/{specification.Namespace}/{specification.TypeName}/{method}";

    void should_select_one_method() => Select(PathFor("should_be_the_first_fact")).Single().Facts.Single().MethodName.ShouldEqual("should_be_the_first_fact");

    void should_select_all_methods_of_a_class() => Select(PathFor("*")).Single().Facts.Count.ShouldEqual(specification.Facts.Count);

    void should_select_by_namespace() => Select($"/Probe.Assembly/{specification.Namespace}/*/*").Count.ShouldBeGreaterThan(0);

    void should_match_wildcards() => Select(PathFor("should_be_the_*_fact")).Single().Facts.Count.ShouldEqual(2);

    void should_leave_no_match_empty() => Select(PathFor("no_such_method")).ShouldBeEmpty();

    void should_not_ignore_the_assembly_segment() => Select(PathFor("*").Replace("Probe.Assembly", "Other.Assembly", StringComparison.Ordinal)).ShouldBeEmpty();

    void should_combine_alternatives_in_one_segment() => Select(PathFor("(should_be_the_first_fact|should_be_the_second_fact)")).Single().Facts.Count.ShouldEqual(2);
}
