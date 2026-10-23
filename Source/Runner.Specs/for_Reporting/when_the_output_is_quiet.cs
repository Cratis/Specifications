// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_Reporting;

public class when_the_output_is_quiet : Specification
{
    ReportingResultSink quiet;
    ReportingResultSink off;
    string rendered;

    void Because()
    {
        var output = new recording_output();
        var inner = new given.a_recording_sink();
        quiet = ReportingResultSink.Create(inner, new(OutputLevel.Quiet, false, false, false, null, []), false, output, _ => null);
        off = ReportingResultSink.Create(inner, SpecificationReportOptions.Off, false, output, _ => null);
        rendered = approved_snapshot.Join(output.Lines);
    }

    void should_register_no_consumer() => quiet.ShouldBeNull();

    void should_register_no_consumer_for_the_default_options() => off.ShouldBeNull();

    void should_print_nothing() => rendered.Trim().ShouldBeEmpty();
}
