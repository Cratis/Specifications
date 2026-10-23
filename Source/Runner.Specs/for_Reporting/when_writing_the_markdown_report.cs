// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_Reporting;

public class when_writing_the_markdown_report : Specification
{
    string result;

    void Because() => result = MarkdownReportWriter.Render(reporting_fixture.Tree());

    void should_match_the_snapshot() => result.ShouldEqual(approved_snapshot.Approved("report.md", result));
}
