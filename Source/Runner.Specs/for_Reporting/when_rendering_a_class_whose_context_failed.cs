// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_Reporting;

public class when_rendering_a_class_whose_context_failed : Specification
{
    string result;

    void Because() => result = rendered_tree.Render(new(OutputLevel.Normal, false, false, false, null, []));

    void should_report_the_failure_once() => result.Split("The database was not available").Length.ShouldEqual(2);

    void should_report_it_on_the_context() => result.ShouldContain("The context failed");

    void should_not_report_it_on_the_facts() => ClassReport.From(reporting_fixture.Classes()[1].Work, reporting_fixture.Classes()[1].Results).Facts.All(_ => _.Failure is null).ShouldBeTrue();
}
