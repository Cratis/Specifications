// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_Specification.probes;

/// <summary>
/// Tracks the Destroy of a specification running for real on TUnit, and fails the run when it did not run exactly once.
/// </summary>
public static class destroy_tracker
{
    static int _destroyCalls;

    public static void Destroyed() => Interlocked.Increment(ref _destroyCalls);

    [After(HookType.Assembly)]
    public static void VerifyDestroyedOnce(AssemblyHookContext context)
    {
        if (_destroyCalls != 1)
        {
            throw new InvalidOperationException($"Expected Destroy to run once but it ran {_destroyCalls} times");
        }
    }
}
