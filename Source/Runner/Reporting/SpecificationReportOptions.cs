// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using Microsoft.Testing.Platform.CommandLine;

namespace Cratis.Specifications.Runner;

/// <summary>
/// Represents the options controlling the console tree and the reports of the runner.
/// </summary>
/// <param name="Level">How much the console tree prints.</param>
/// <param name="NoColor">Whether to print without color.</param>
/// <param name="Ascii">Whether to use plain ASCII markers.</param>
/// <param name="Timings">Whether phase timings were requested on the command line.</param>
/// <param name="Slow">The duration from which a phase is shown as slow; null for the default.</param>
/// <param name="Reports">The reports to write.</param>
internal sealed record SpecificationReportOptions(OutputLevel Level, bool NoColor, bool Ascii, bool Timings, TimeSpan? Slow, IReadOnlyList<ReportRequest> Reports)
{
    /// <summary>
    /// Gets the options that print nothing and write nothing.
    /// </summary>
    public static SpecificationReportOptions Off { get; } = new(OutputLevel.Quiet, true, true, false, null, []);

    /// <summary>
    /// Gets a value indicating whether the console tree is wanted, before taking server mode into account.
    /// </summary>
    public bool WantsConsole => Level != OutputLevel.Quiet;

    /// <summary>
    /// Gets the duration from which a phase is shown as slow.
    /// </summary>
    public TimeSpan SlowThreshold => Slow ?? PhaseTimingOptions.DefaultSlowThreshold;

    /// <summary>
    /// Parses an output level.
    /// </summary>
    /// <param name="value">The text.</param>
    /// <param name="level">The level.</param>
    /// <returns>True when the text names a level.</returns>
    public static bool TryParseLevel(string value, out OutputLevel level)
    {
        level = OutputLevel.Normal;
        switch (value.ToLowerInvariant())
        {
            case "quiet":
                level = OutputLevel.Quiet;
                return true;
            case "normal":
                return true;
            case "detailed":
                level = OutputLevel.Detailed;
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Reads the options from the command line and the environment.
    /// </summary>
    /// <param name="commandLine">The parsed command line.</param>
    /// <param name="environment">Reads an environment variable.</param>
    /// <returns>The options.</returns>
    public static SpecificationReportOptions Read(ICommandLineOptions commandLine, Func<string, string?> environment)
    {
        var level = OutputLevel.Normal;
        if (commandLine.TryGetOptionArgumentList(SpecificationCommandLineOptions.Output, out var output)) _ = TryParseLevel(output[0], out level);

        TimeSpan? slow = commandLine.TryGetOptionArgumentList(SpecificationCommandLineOptions.Slow, out var slowArguments)
            ? TimeSpan.FromMilliseconds(int.Parse(slowArguments[0], CultureInfo.InvariantCulture))
            : null;

        IReadOnlyList<ReportRequest> reports = [];
        if (commandLine.TryGetOptionArgumentList(SpecificationCommandLineOptions.Report, out var reportArguments)) _ = ReportRequest.TryParse(reportArguments, out reports);

        var noColor = commandLine.IsOptionSet(SpecificationCommandLineOptions.NoColor) || !string.IsNullOrEmpty(environment("NO_COLOR"));
        return new(level, noColor, !IsUnicodeConsole(), commandLine.IsOptionSet(SpecificationCommandLineOptions.Timings), slow, reports);
    }

    static bool IsUnicodeConsole()
    {
        try
        {
            return Console.OutputEncoding.WebName.StartsWith("utf", StringComparison.OrdinalIgnoreCase);
        }
        catch (IOException)
        {
            return true;
        }
    }
}
