// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.Observation;

namespace Cratis.Specifications.Runner.for_SpecificationObservers.given;

/// <summary>
/// Builds specification work with the observation catalog's lifecycle by hand.
/// </summary>
internal static class observed_work
{
    public static SpecificationWork For(string uid, bool skipped = false, Func<object> create = null)
    {
        var fact = new FactDescriptor("should_hold", $"{uid} should hold", null, 0, _ => ValueTask.CompletedTask, skipped ? "Probe" : null);
        var specification = new SpecificationClassDescriptor(
            uid,
            "Observation",
            uid,
            uid,
            create ?? (() => new object()),
            _ => Run(ObservedBehavior.Establish),
            _ => Run(ObservedBehavior.Because),
            _ => Run(ObservedBehavior.Destroy),
            [fact]);
        return new(specification, specification.Facts);
    }

    static ValueTask Run(Action phase)
    {
        phase();
        return ValueTask.CompletedTask;
    }
}
