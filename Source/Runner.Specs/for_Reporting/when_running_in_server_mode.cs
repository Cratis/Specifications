// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_Reporting;

public class when_running_in_server_mode : Specification
{
    recording_output output;
    ReportingResultSink withoutReports;
    ReportingResultSink withReports;

    async Task Because()
    {
        output = new recording_output();
        var inner = new given.a_recording_sink();
        var normal = new SpecificationReportOptions(OutputLevel.Normal, false, false, false, null, []);
        withoutReports = ReportingResultSink.Create(inner, normal, true, output, _ => null);
        withReports = ReportingResultSink.Create(inner, normal with { Reports = [new(ReportRequest.GitHub, null)] }, true, output, _ => null);
        var (work, results) = reporting_fixture.Classes()[0];
        await withReports.Completed(work, results);
    }

    void Destroy() => withReports.Dispose();

    void should_not_render_the_console_tree() => output.Lines.ShouldBeEmpty();

    void should_register_nothing_without_reports() => withoutReports.ShouldBeNull();

    void should_still_collect_requested_reports() => withReports.ShouldNotBeNull();
}
