// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_SpecificationGenerator;

public class when_registering_specifications : Specification
{
    IReadOnlyList<string> uids;

    void Because() => uids = [.. given.the_probes.All.Select(_ => _.Uid)];

    void should_leave_out_classes_without_facts() => uids.ShouldNotContain(typeof(without_facts).FullName);

    void should_leave_out_base_classes_without_facts() => uids.ShouldNotContain(typeof(a_recording_context).FullName);

    void should_order_the_classes_by_uid() => uids.ShouldEqual(uids.Order(StringComparer.Ordinal).ToList());

    void should_not_register_the_probes_as_specifications_of_the_running_assembly() => SpecificationRegistry.Classes.Select(_ => _.Uid).ShouldNotContain(typeof(failing_because).FullName);
}
