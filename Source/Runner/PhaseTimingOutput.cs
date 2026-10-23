// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Cratis.Specifications.Runner;

/// <summary>
/// Starts phase timings for the native runner when they are enabled. The runner reports them as the steps of every fact
/// node, so it has no slow phase channel of its own.
/// </summary>
internal static class PhaseTimingOutput
{
    /// <summary>
    /// Starts phase timings when they are enabled; with them off nothing is registered.
    /// </summary>
#pragma warning disable CA2255 // The runner starts phase timings when its assembly loads, before the first specification runs.
    [ModuleInitializer]
    internal static void Register() => PhaseTimings.Initialize(null);
#pragma warning restore CA2255
}
