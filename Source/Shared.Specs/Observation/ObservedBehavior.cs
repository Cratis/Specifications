// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Observation;

/// <summary>
/// The lifecycle behavior of the observation probes, shared by every package's probe.
/// </summary>
internal static class ObservedBehavior
{
    public static ObservedFailure Failure { get; set; }

    public static void Establish() => FailIf(ObservedFailure.Establish);

    public static void Because()
    {
        FailIf(ObservedFailure.Because);
        if (Failure == ObservedFailure.Cancelled) throw new OperationCanceledException("Observed cancellation");
    }

    public static void Destroy() => FailIf(ObservedFailure.Destroy);

    static void FailIf(ObservedFailure failure)
    {
        if (Failure == failure) throw new InvalidOperationException($"Observed {failure} failure");
    }
}
