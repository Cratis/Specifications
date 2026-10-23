// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Specifications.Runner.for_Reporting;

public class when_writing_the_json_report : Specification
{
    string result;
    JsonElement document;

    void Because()
    {
        result = JsonReportWriter.Render(reporting_fixture.Tree());
        document = JsonDocument.Parse(result).RootElement;
    }

    void should_match_the_snapshot() => result.ShouldEqual(approved_snapshot.Approved("report.json", result));

    void should_share_the_schema_version_of_the_timings_file() => document.GetProperty("schema").GetInt32().ShouldEqual(TimingsJsonWriter.SchemaVersion);

    void should_summarize_the_outcomes() => (document.GetProperty("summary").GetProperty("passed").GetInt32(), document.GetProperty("summary").GetProperty("failed").GetInt32(), document.GetProperty("summary").GetProperty("skipped").GetInt32()).ShouldEqual((2, 3, 1));

    void should_describe_every_node_with_a_name_and_a_kind() => Nodes(document.GetProperty("specifications")).All(_ => _.GetProperty("name").ValueKind == JsonValueKind.String && _.GetProperty("kind").GetString() is "group" or "fact").ShouldBeTrue();

    void should_give_every_fact_an_outcome_and_a_duration() => Nodes(document.GetProperty("specifications"))
        .Where(_ => _.GetProperty("kind").GetString() == "fact")
        .All(_ => _.GetProperty("outcome").ValueKind == JsonValueKind.String && _.GetProperty("durationMs").ValueKind == JsonValueKind.Number).ShouldBeTrue();

    void should_write_phases_with_the_property_names_of_the_timings_file() =>
        Nodes(document.GetProperty("specifications")).First(_ => _.TryGetProperty("phases", out _)).GetProperty("phases")[0].EnumerateObject().Select(_ => _.Name)
            .ShouldContainOnly("phase", "owner", "specification", "startedUtc", "durationMs", "failed");

    static IEnumerable<JsonElement> Nodes(JsonElement array)
    {
        foreach (var node in array.EnumerateArray())
        {
            yield return node;
            if (node.TryGetProperty("children", out var children))
            {
                foreach (var child in Nodes(children)) yield return child;
            }
        }
    }
}
