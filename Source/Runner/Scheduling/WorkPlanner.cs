// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner;

/// <summary>
/// Plans a run: picks the classes of the current shard, by a hash of the unique identifier so that every process agrees on the split whatever its run cache holds, and orders them so the run finishes as early as possible.
/// </summary>
internal static class WorkPlanner
{
    /// <summary>
    /// Plans the given work.
    /// </summary>
    /// <param name="work">The selected work, ordered by unique identifier.</param>
    /// <param name="options">The run options.</param>
    /// <param name="cache">What earlier runs remembered.</param>
    /// <returns>The work to run, in the order to start it.</returns>
    public static IReadOnlyList<SpecificationWork> Plan(IReadOnlyList<SpecificationWork> work, SpecificationRunOptions options, RunCache cache)
    {
        var fallback = FallbackEstimate(work, cache);
        var estimated = new List<(SpecificationWork Work, double Estimate, bool Known, bool Failed)>(work.Count);
        foreach (var item in work)
        {
            var known = cache.TryGet(item.Specification.Uid, out var entry);
            estimated.Add((item, known ? entry.Milliseconds : fallback, known, known && entry.Failed));
        }

        if (options.Shard is Shard shard && shard.Count > 1)
        {
            estimated = SplitByHash(estimated, shard);
        }

        if (!cache.HasHistory && !options.LastFailedFirst) return estimated.ConvertAll(_ => _.Work);

        estimated.Sort((left, right) =>
        {
            if (options.LastFailedFirst && left.Failed != right.Failed) return left.Failed ? -1 : 1;
            if (left.Known != right.Known) return left.Known ? 1 : -1;
            var byDuration = right.Estimate.CompareTo(left.Estimate);
            return byDuration != 0 ? byDuration : string.CompareOrdinal(left.Work.Specification.Uid, right.Work.Specification.Uid);
        });
        return estimated.ConvertAll(_ => _.Work);
    }

    /// <summary>
    /// Computes a stable hash of a string, identical across processes, runtimes and operating systems.
    /// </summary>
    /// <param name="value">The value to hash.</param>
    /// <returns>The 32-bit FNV-1a hash.</returns>
    public static uint StableHash(string value)
    {
        var hash = 2166136261u;
        foreach (var character in value)
        {
            hash = (hash ^ character) * 16777619u;
        }

        return hash;
    }

    static List<(SpecificationWork Work, double Estimate, bool Known, bool Failed)> SplitByHash(
        List<(SpecificationWork Work, double Estimate, bool Known, bool Failed)> estimated,
        Shard shard) =>
        estimated.FindAll(_ => (StableHash(_.Work.Specification.Uid) % (uint)shard.Count) == (uint)(shard.Index - 1));

    static double FallbackEstimate(IReadOnlyList<SpecificationWork> work, RunCache cache)
    {
        var known = new List<double>();
        foreach (var item in work)
        {
            if (cache.TryGet(item.Specification.Uid, out var entry)) known.Add(entry.Milliseconds);
        }

        if (known.Count == 0) return 1;
        known.Sort();
        return known[known.Count / 2];
    }
}
