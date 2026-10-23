// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using System.Text.Json;

namespace Cratis.Specifications.Runner;

/// <summary>
/// Writes the specification tree as JSON. The schema number and the phase objects are shared with the phase timings file.
/// </summary>
internal static class JsonReportWriter
{
    static readonly JsonWriterOptions _options = new() { Indented = true, NewLine = "\n" };

    /// <summary>
    /// Renders the tree.
    /// </summary>
    /// <param name="tree">The tree.</param>
    /// <returns>The JSON.</returns>
    public static string Render(ReportTree tree)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, _options))
        {
            writer.WriteStartObject();
            writer.WriteNumber("schema", TimingsJsonWriter.SchemaVersion);
            writer.WriteStartObject("summary");
            writer.WriteNumber("passed", tree.Passed);
            writer.WriteNumber("failed", tree.Failed);
            writer.WriteNumber("skipped", tree.Skipped);
            writer.WriteNumber("durationMs", Milliseconds(tree.Duration));
            writer.WriteEndObject();
            writer.WriteStartArray("specifications");
            foreach (var node in tree.Roots.OrderBy(_ => _.Name, StringComparer.Ordinal)) Write(writer, node);
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray()) + "\n";
    }

    static double Milliseconds(TimeSpan duration) => Math.Round(duration.TotalMilliseconds, 3);

    static void Write(Utf8JsonWriter writer, ReportNode node)
    {
        writer.WriteStartObject();
        writer.WriteString("name", node.Name);
        writer.WriteString("kind", node.IsFact ? "fact" : "group");
        if (node.IsFact)
        {
            writer.WriteString("outcome", node.Outcome.ToString().ToLowerInvariant());
            writer.WriteNumber("durationMs", Milliseconds(node.Duration));
            if (node.FilePath is not null)
            {
                writer.WriteString("file", node.FilePath);
                writer.WriteNumber("line", node.Line);
            }

            if (node.SkipReason is not null) writer.WriteString("skipReason", node.SkipReason);
            if (node.Failure is not null) WriteFailure(writer, "failure", node.Failure);
            WritePhases(writer, node.Phases);
        }
        else
        {
            if (node.ContextFailure is not null) WriteFailure(writer, "contextFailure", node.ContextFailure);
            writer.WriteStartArray("children");
            foreach (var child in node.OrderedChildren()) Write(writer, child);
            writer.WriteEndArray();
        }

        writer.WriteEndObject();
    }

    static void WriteFailure(Utf8JsonWriter writer, string name, FailureInfo failure)
    {
        writer.WriteStartObject(name);
        writer.WriteString("message", failure.Message);
        if (failure.Expected is not null) writer.WriteString("expected", failure.Expected);
        if (failure.Actual is not null) writer.WriteString("actual", failure.Actual);
        writer.WriteEndObject();
    }

    static void WritePhases(Utf8JsonWriter writer, IReadOnlyList<PhaseRecord>? phases)
    {
        if (phases is null) return;
        writer.WriteStartArray("phases");
        foreach (var record in phases)
        {
            writer.WriteStartObject();
            writer.WriteString("phase", record.Phase.ToString());
            writer.WriteString("owner", record.Owner);
            if (record.Specification is not null) writer.WriteString("specification", record.Specification);
            if (record.ResourceKey is not null) writer.WriteString("resourceKey", record.ResourceKey);
            writer.WriteString("startedUtc", record.StartedUtc);
            writer.WriteNumber("durationMs", Milliseconds(record.Duration));
            writer.WriteBoolean("failed", record.Failed);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }
}
