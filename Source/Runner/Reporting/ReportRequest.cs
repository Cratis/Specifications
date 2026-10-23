// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner;

/// <summary>
/// Represents a report requested with --specs-report.
/// </summary>
/// <param name="Format">The format: md, json or github.</param>
/// <param name="Path">The file to write; null for github, which writes to the file named by GITHUB_STEP_SUMMARY.</param>
internal sealed record ReportRequest(string Format, string? Path)
{
    /// <summary>The Markdown format.</summary>
    public const string Markdown = "md";

    /// <summary>The JSON format.</summary>
    public const string Json = "json";

    /// <summary>The GitHub step summary format.</summary>
    public const string GitHub = "github";

    /// <summary>
    /// Gets the environment variable naming the GitHub step summary file.
    /// </summary>
    public const string GitHubSummaryVariable = "GITHUB_STEP_SUMMARY";

    /// <summary>
    /// Parses the arguments of --specs-report: one or more of "md &lt;path&gt;", "json &lt;path&gt;" and "github".
    /// </summary>
    /// <param name="arguments">The arguments.</param>
    /// <param name="requests">The requested reports.</param>
    /// <returns>True when the arguments are valid.</returns>
    public static bool TryParse(string[] arguments, out IReadOnlyList<ReportRequest> requests)
    {
        var parsed = new List<ReportRequest>();
        requests = parsed;
        for (var index = 0; index < arguments.Length; index++)
        {
            var format = arguments[index].ToLowerInvariant();
            if (format == GitHub)
            {
                parsed.Add(new(GitHub, null));
            }
            else if ((format == Markdown || format == Json) && index + 1 < arguments.Length)
            {
                parsed.Add(new(format, arguments[++index]));
            }
            else
            {
                return false;
            }
        }

        return parsed.Count > 0;
    }
}
