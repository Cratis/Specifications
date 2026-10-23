// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_PhaseTimings;

public class when_writing_the_json : Specification
{
    // The checked-in snapshot of the schema; the shared JSON report reuses it, so changing it is a breaking change.
    const string Snapshot = """
        {
          "schema": 1,
          "phases": [
            {
              "phase": "Establish",
              "owner": "spec",
              "specification": "a spec",
              "startedUtc": "2026-01-02T03:04:05+00:00",
              "durationMs": 12.5,
              "failed": false
            },
            {
              "phase": "ResourceStart",
              "owner": "key",
              "resourceKey": "database",
              "startedUtc": "2026-01-02T03:04:06+00:00",
              "durationMs": 1500,
              "failed": true
            }
          ]
        }
        """;

    string json;
    string written_to_a_file;

    void Because()
    {
        PhaseRecord[] records =
        [
            new(SpecificationPhase.Establish, PhaseRecord.SpecOwner, "a spec", null, new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero), TimeSpan.FromMilliseconds(12.5), false),
            new(SpecificationPhase.ResourceStart, PhaseRecord.KeyOwner, null, "database", new DateTimeOffset(2026, 1, 2, 3, 4, 6, TimeSpan.Zero), TimeSpan.FromMilliseconds(1500), true)
        ];

        using var stream = new MemoryStream();
        TimingsJsonWriter.Write(stream, records);
        json = System.Text.Encoding.UTF8.GetString(stream.ToArray());

        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "timings.json");
        try
        {
            TimingsJsonWriter.Write(path, records);
            written_to_a_file = File.ReadAllText(path);
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(path), true);
        }
    }

    void should_match_the_schema_snapshot() => json.ReplaceLineEndings("\n").ShouldEqual(Snapshot.ReplaceLineEndings("\n"));
    void should_write_the_same_document_to_a_file_creating_its_directory() => written_to_a_file.ShouldEqual(json);
}
