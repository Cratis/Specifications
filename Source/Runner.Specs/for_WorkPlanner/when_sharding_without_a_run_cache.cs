// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.Runner.given;

namespace Cratis.Specifications.Runner.for_WorkPlanner;

public class when_sharding_without_a_run_cache : Specification
{
    IReadOnlyList<SpecificationWork> all;
    List<IReadOnlyList<string>> shards;

    void Establish() => all = given.specification_work.Many(100);

    void Because() => shards = [.. Enumerable.Range(1, 4)
        .Select(_ => WorkPlanner.Plan(all, SpecificationRunOptions.Default with { Shard = new Shard(_, 4) }, RunCache.Empty).Uids())];

    void should_run_every_class_in_exactly_one_shard() => shards.SelectMany(_ => _).Order(StringComparer.Ordinal).ShouldEqual(all.Uids());

    void should_give_every_shard_some_classes() => shards.ShouldEachConformTo(_ => _.Count > 10);

    void should_split_the_same_way_every_time() =>
        WorkPlanner.Plan(all, SpecificationRunOptions.Default with { Shard = new Shard(2, 4) }, RunCache.Empty).Uids().ShouldEqual(shards[1]);

    void should_use_a_hash_that_is_stable_across_runs() => WorkPlanner.StableHash("for_Author.when_registering").ShouldEqual(WorkPlanner.StableHash("for_Author.when_registering"));
}
