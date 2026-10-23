// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.Runner.given;

namespace Cratis.Specifications.Runner.for_WorkPlanner;

public class when_planning_with_a_run_cache : Specification
{
    string path;
    IReadOnlyList<SpecificationWork> all;
    RunCache cache;
    IReadOnlyList<string> longest_first;
    IReadOnlyList<string> last_failed_first;
    List<IReadOnlyList<string>> shards;

    void Establish()
    {
        path = Path.Combine(Path.GetTempPath(), $"specs-cache-{Guid.NewGuid():N}");
        all = [given.specification_work.For("fast"), given.specification_work.For("slow"), given.specification_work.For("failed"), given.specification_work.For("new"), given.specification_work.For("medium")];
        var recording = RunCache.Empty;
        recording.Record("fast", TimeSpan.FromMilliseconds(1), false);
        recording.Record("slow", TimeSpan.FromMilliseconds(100), false);
        recording.Record("failed", TimeSpan.FromMilliseconds(2), true);
        recording.Record("medium", TimeSpan.FromMilliseconds(50), false);
        recording.Save(path);
    }

    void Because()
    {
        cache = RunCache.Load(path);
        longest_first = WorkPlanner.Plan(all, SpecificationRunOptions.Default, cache).Uids();
        last_failed_first = WorkPlanner.Plan(all, SpecificationRunOptions.Default with { LastFailedFirst = true }, cache).Uids();
        shards = [.. Enumerable.Range(1, 2)
            .Select(_ => WorkPlanner.Plan(all, SpecificationRunOptions.Default with { Shard = new Shard(_, 2) }, cache).Uids())];
    }

    void Destroy() => File.Delete(path);

    void should_remember_the_previous_run() => cache.HasHistory.ShouldBeTrue();

    void should_start_unknown_classes_and_then_the_longest_classes_first() => longest_first.ShouldEqual("new", "slow", "medium", "failed", "fast");

    void should_start_the_classes_that_failed_last_time_first_when_asked() => last_failed_first[0].ShouldEqual("failed");

    void should_split_the_shards_by_hash_regardless_of_the_cache() =>
        shards.SelectMany(_ => _).Order(StringComparer.Ordinal).ShouldEqual(all.Uids().Order(StringComparer.Ordinal));
}
