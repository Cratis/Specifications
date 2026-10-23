// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Xunit;

namespace Cratis.Specifications;

/// <summary>
/// Reports slow specification phases through xUnit v3 diagnostic messages and test output when phase timings are on.
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
    /// Reports a line as a diagnostic message and as test output of the running test.
    /// </summary>
    /// <param name="line">The line.</param>
    internal static void Write(string line)
    {
        var context = TestContext.Current;
        context.SendDiagnosticMessage(line);
        context.TestOutputHelper?.WriteLine(line);
    }
}
