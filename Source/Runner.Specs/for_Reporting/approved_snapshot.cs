// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Cratis.Specifications.Runner.for_Reporting;

/// <summary>
/// Compares text with the approved snapshot next to the specs, read from the copy in the output folder. Set CRATIS_SPECS_UPDATE_SNAPSHOTS=1 to rewrite the snapshots.
/// </summary>
internal static class approved_snapshot
{
    public static string Approved(string name, string actual, [CallerFilePath] string source = "")
    {
        if (Environment.GetEnvironmentVariable("CRATIS_SPECS_UPDATE_SNAPSHOTS") == "1")
        {
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(source), "Snapshots", name), actual);
            return actual;
        }

        // The snapshots are copied to the output; source paths are mapped (/_/) in CI builds and cannot be read.
        var path = Path.Combine(AppContext.BaseDirectory, "for_Reporting", "Snapshots", name);
        return File.ReadAllText(path).Replace("\r\n", "\n");
    }

    public static string Join(IEnumerable<OutputLine> lines) => string.Join("\n", lines.Select(_ => _.Text)) + "\n";
}
