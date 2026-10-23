// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_SpecificationScheduler;

public class when_failing_fast : Specification
{
    given.a_recording_sink sink;
    IReadOnlyList<SpecificationWork> not_started;

    void Establish() => sink = new();

    async Task Because()
    {
        var plan = new[] { given.specification_work.For("first", fails: true) }.Concat(given.specification_work.Many(10)).ToList();
        not_started = await SpecificationScheduler.Run(plan, SpecificationRunOptions.Default with { MaxParallelClasses = 1, FailFast = true }, sink, RunCache.Empty, CancellationToken.None);
    }

    void should_complete_only_the_failing_class() => sink.Completed.Select(_ => _.Work.Specification.Uid).ShouldContainOnly("first");

    void should_report_the_rest_as_not_started() => not_started.Count.ShouldEqual(10);
}
