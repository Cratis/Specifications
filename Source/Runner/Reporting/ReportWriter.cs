// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner;

/// <summary>
/// Writes the requested reports once the run has completed.
/// </summary>
internal static class ReportWriter
{
    /// <summary>
    /// Writes the reports. A report that cannot be written is described in the returned messages; it never fails the run.
    /// </summary>
    /// <param name="requests">The requested reports.</param>
    /// <param name="tree">The tree.</param>
    /// <param name="environment">Reads an environment variable.</param>
    /// <returns>The messages describing reports that could not be written.</returns>
    public static IReadOnlyList<string> Write(IReadOnlyList<ReportRequest> requests, ReportTree tree, Func<string, string?> environment)
    {
        var problems = new List<string>();
        foreach (var request in requests)
        {
            try
            {
                WriteOne(request, tree, environment);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                problems.Add($"Cratis.Specifications: could not write the {request.Format} report ({ex.GetType().Name})");
            }
        }

        return problems;
    }

    static void WriteOne(ReportRequest request, ReportTree tree, Func<string, string?> environment)
    {
        switch (request.Format)
        {
            case ReportRequest.Markdown:
                WriteFile(request.Path!, MarkdownReportWriter.Render(tree), false);
                break;
            case ReportRequest.Json:
                WriteFile(request.Path!, JsonReportWriter.Render(tree), false);
                break;
            default:
                if (environment(ReportRequest.GitHubSummaryVariable) is { Length: > 0 } summary) WriteFile(summary, MarkdownReportWriter.Render(tree), true);
                break;
        }
    }

    static void WriteFile(string path, string content, bool append)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        if (append) File.AppendAllText(path, content);
        else File.WriteAllText(path, content);
    }
}
