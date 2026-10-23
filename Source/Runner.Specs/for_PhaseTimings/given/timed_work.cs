// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_PhaseTimings.given;

/// <summary>
/// Builds specification work with a lifecycle that is not shared with other specifications, optionally failing Because. Every phase takes measurable time.
/// </summary>
internal static class timed_work
{
    public static SpecificationWork For(string uid, bool failBecause = false)
    {
        var fact = new FactDescriptor("should_hold", $"{uid} should hold", null, 0, _ => ValueTask.CompletedTask);
        var specification = new SpecificationClassDescriptor(
            uid,
            "Timings",
            uid,
            uid,
            () => new object(),
            _ => Elapse(),
            _ => failBecause ? throw new InvalidOperationException("Observed failure text") : Elapse(),
            _ => Elapse(),
            [fact]);
        return new(specification, specification.Facts);
    }

    // A phase that returns at once can measure as zero, since a TimeSpan only resolves 100 ns, and zero is never above a zero threshold.
    static ValueTask Elapse() => new(Task.Delay(1));
}
