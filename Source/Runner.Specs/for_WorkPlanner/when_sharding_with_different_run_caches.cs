// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.Runner.given;

namespace Cratis.Specifications.Runner.for_WorkPlanner;

public class when_sharding_with_different_run_caches : Specification
{
    IReadOnlyList<SpecificationWork> all;
    List<IReadOnlyList<string>> shards;

    void Establish() => all = [.. new[] { "a", "b", "c", "d" }.Select(_ => given.specification_work.For(_))];

    void Because() => shards = [.. Enumerable.Range(1, 2).Select(index => WorkPlanner.Plan(all, SpecificationRunOptions.Default with { Shard = new Shard(index, 2) }, CacheOf(index)).Uids())];

    void should_give_every_class_to_exactly_one_shard() => shards.SelectMany(_ => _).Order(StringComparer.Ordinal).ShouldEqual(all.Uids());

    void should_give_each_class_to_the_shard_its_hash_selects() =>
        shards.ShouldEachConformTo(shard => shard.All(uid => shards[(int)(WorkPlanner.StableHash(uid) % 2)] == shard));

    // Every process remembers a different subset of classes, with very different durations.
    static RunCache CacheOf(int index)
    {
        var path = Path.Combine(Path.GetTempPath(), $"specs-cache-{Guid.NewGuid():N}");
        try
        {
            var recording = RunCache.Empty;
            if (index == 1)
            {
                recording.Record("a", TimeSpan.FromMilliseconds(1), false);
                recording.Record("b", TimeSpan.FromMilliseconds(1), false);
                recording.Record("c", TimeSpan.FromMilliseconds(1), false);
                recording.Record("d", TimeSpan.FromMilliseconds(10), false);
            }
            else
            {
                recording.Record("d", TimeSpan.FromMilliseconds(1), false);
                recording.Record("a", TimeSpan.FromMilliseconds(10), false);
            }

            recording.Save(path);
            return RunCache.Load(path);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
