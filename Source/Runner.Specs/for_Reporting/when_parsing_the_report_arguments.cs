// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_Reporting;

public class when_parsing_the_report_arguments : Specification
{
    bool several;
    IReadOnlyList<ReportRequest> requests;
    bool withoutPath;
    bool unknown;
    bool level;
    bool badLevel;

    void Because()
    {
        several = ReportRequest.TryParse(["md", "a.md", "json", "b.json", "github"], out requests);
        withoutPath = ReportRequest.TryParse(["md"], out _);
        unknown = ReportRequest.TryParse(["html", "x"], out _);
        level = SpecificationReportOptions.TryParseLevel("Detailed", out var parsed) && parsed == OutputLevel.Detailed;
        badLevel = SpecificationReportOptions.TryParseLevel("loud", out _);
    }

    void should_accept_several_reports() => several.ShouldBeTrue();

    void should_return_them_in_order() => requests.Select(_ => (_.Format, _.Path)).ShouldEqual([("md", "a.md"), ("json", "b.json"), ("github", null)]);

    void should_reject_a_file_report_without_a_path() => withoutPath.ShouldBeFalse();

    void should_reject_an_unknown_format() => unknown.ShouldBeFalse();

    void should_parse_a_level_ignoring_case() => level.ShouldBeTrue();

    void should_reject_an_unknown_level() => badLevel.ShouldBeFalse();
}
