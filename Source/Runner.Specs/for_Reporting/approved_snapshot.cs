// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Cratis.Specifications.Runner.for_Reporting;

/// <summary>
/// Compares text with the approved snapshot next to the specs. Set CRATIS_SPECS_UPDATE_SNAPSHOTS=1 to rewrite the snapshots.
/// </summary>
internal static class approved_snapshot
{
    public static string Approved(string name, string actual, [CallerFilePath] string source = "")
    {
        var path = Path.Combine(Path.GetDirectoryName(source), "Snapshots", name);
        if (Environment.GetEnvironmentVariable("CRATIS_SPECS_UPDATE_SNAPSHOTS") == "1") File.WriteAllText(path, actual);
        return File.ReadAllText(path).Replace("\r\n", "\n");
    }

    public static string Join(IEnumerable<OutputLine> lines) => string.Join("\n", lines.Select(_ => _.Text)) + "\n";
}
