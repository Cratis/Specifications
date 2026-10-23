// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Specifications;

/// <summary>
/// Writes the phase timings of a test session as JSON.
/// </summary>
/// <remarks>
/// The document is <c>{ "schema": 1, "phases": [ { phase, owner, specification?, resourceKey?, startedUtc, durationMs, failed } ] }</c>.
/// The schema is shared with the JSON report.
/// </remarks>
internal static class TimingsJsonWriter
{
    /// <summary>The version of the JSON schema.</summary>
    public const int SchemaVersion = 1;

    static readonly JsonWriterOptions _options = new() { Indented = true };

    /// <summary>
    /// Writes the records to a stream.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="records">The records.</param>
    public static void Write(Stream stream, IReadOnlyList<PhaseRecord> records)
    {
        using var writer = new Utf8JsonWriter(stream, _options);
        writer.WriteStartObject();
        writer.WriteNumber("schema", SchemaVersion);
        writer.WriteStartArray("phases");
        foreach (var record in records)
        {
            writer.WriteStartObject();
            writer.WriteString("phase", record.Phase.ToString());
            writer.WriteString("owner", record.Owner);
            if (record.Specification is not null) writer.WriteString("specification", record.Specification);
            if (record.ResourceKey is not null) writer.WriteString("resourceKey", record.ResourceKey);
            writer.WriteString("startedUtc", record.StartedUtc);
            writer.WriteNumber("durationMs", Math.Round(record.Duration.TotalMilliseconds, 3));
            writer.WriteBoolean("failed", record.Failed);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    /// <summary>
    /// Writes the records to a file, creating its directory.
    /// </summary>
    /// <param name="path">The path of the file.</param>
    /// <param name="records">The records.</param>
    public static void Write(string path, IReadOnlyList<PhaseRecord> records)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        using var stream = File.Create(path);
        Write(stream, records);
    }
}
