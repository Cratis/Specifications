// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_Reporting;

public class when_rendering_without_color_or_unicode : Specification
{
    string result;
    recording_output output;
    ReportingResultSink sink;

    async Task Because()
    {
        var options = new SpecificationReportOptions(OutputLevel.Normal, true, true, false, null, []);
        result = rendered_tree.Render(options);
        output = new recording_output();
        sink = ReportingResultSink.Create(new given.a_recording_sink(), options, false, output, _ => null);
        foreach (var (work, results) in reporting_fixture.Classes()) await sink.Completed(work, results);
    }

    void should_match_the_snapshot() => result.ShouldEqual(approved_snapshot.Approved("normal-ascii.txt", result));

    void should_only_use_ascii() => result.All(_ => _ < 128).ShouldBeTrue();

    void should_not_ask_for_color() => output.UsedColor.ShouldBeFalse();
}
