// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace Cratis.Specifications;

/// <summary>
/// Reports slow specification phases through NUnit progress output when phase timings are on.
/// </summary>
internal static class PhaseTimingOutput
{

    /// <summary>
    /// Starts phase timings when they are enabled; with them off nothing is registered.
    /// </summary>
#pragma warning disable CA2255 // The packages start phase timings when their assembly loads, before the first specification runs.
    [ModuleInitializer]
    internal static void Register() => PhaseTimings.Initialize(Write);
#pragma warning restore CA2255

    /// <summary>
    /// Reports a line to the NUnit progress output.
    /// </summary>
    /// <param name="line">The line.</param>
    internal static void Write(string line) => Write(TestContext.Progress, line);

    /// <summary>
    /// Reports a line to a writer.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="line">The line.</param>
    internal static void Write(TextWriter writer, string line) => writer.WriteLine(line);
}
