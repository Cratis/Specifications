// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_Reporting;

public class when_writing_the_github_summary : Specification
{
    string directory;
    string summary;
    string unset;
    IReadOnlyList<string> problems;

    void Because()
    {
        directory = Directory.CreateTempSubdirectory("specs-report").FullName;
        var path = Path.Combine(directory, "summary.md");
        var requests = new[] { new ReportRequest(ReportRequest.GitHub, null) };
        problems = ReportWriter.Write(requests, reporting_fixture.Tree(), _ => null);
        unset = File.Exists(path) ? "written" : "not written";
        ReportWriter.Write(requests, reporting_fixture.Tree(), name => name == ReportRequest.GitHubSummaryVariable ? path : null);
        summary = File.ReadAllText(path);
    }

    void Destroy() => Directory.Delete(directory, true);

    void should_not_write_when_the_environment_variable_is_not_set() => unset.ShouldEqual("not written");

    void should_write_markdown_when_it_is_set() => summary.ShouldEqual(MarkdownReportWriter.Render(reporting_fixture.Tree()));

    void should_not_report_problems() => problems.ShouldBeEmpty();
}
