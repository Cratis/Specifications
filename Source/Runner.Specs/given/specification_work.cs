// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;

namespace Cratis.Specifications.Runner.given;

/// <summary>
/// Builds specification work by hand, for the scheduling specifications.
/// </summary>
internal static class specification_work
{
    public static SpecificationWork For(string uid, ConcurrentQueue<string> started = null, Func<ValueTask> because = null, bool fails = false)
    {
        var fact = new FactDescriptor(
            "should_hold",
            $"{uid} should hold",
            null,
            0,
            _ => fails ? throw new InvalidOperationException("Failed") : ValueTask.CompletedTask);
        var specification = new SpecificationClassDescriptor(
            uid,
            "Scheduling",
            uid,
            uid,
            () =>
            {
                started?.Enqueue(uid);
                return new object();
            },
            null,
            because is null ? null : _ => because(),
            null,
            [fact]);
        return new(specification, specification.Facts);
    }

    public static IReadOnlyList<SpecificationWork> Many(int count, string prefix = "class") =>
        [.. Enumerable.Range(0, count).Select(_ => For($"{prefix}{_:D3}"))];

    public static IReadOnlyList<string> Uids(this IEnumerable<SpecificationWork> work) => [.. work.Select(_ => _.Specification.Uid)];
}
