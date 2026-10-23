// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Cratis.Specifications;

/// <summary>
/// Reports slow specification phases through the xUnit diagnostic message sink when phase timings are on.
/// </summary>
internal static class PhaseTimingOutput
{
    static IMessageSink? _sink;

    /// <summary>
    /// Starts phase timings when they are enabled; with them off nothing is registered.
    /// </summary>
#pragma warning disable CA2255 // The packages start phase timings when their assembly loads, before the first specification runs.
    [ModuleInitializer]
    internal static void Register() => PhaseTimings.Initialize(Write);
#pragma warning restore CA2255

    /// <summary>
    /// Sets the diagnostic message sink of the test framework; slow phases are only reported once it is known.
    /// </summary>
    /// <param name="sink">The sink.</param>
    internal static void UseDiagnosticSink(IMessageSink sink) => _sink = sink;

    /// <summary>
    /// Reports a line as a diagnostic message.
    /// </summary>
    /// <param name="line">The line.</param>
    internal static void Write(string line) => _sink?.OnMessage(new DiagnosticMessage(line));
}
