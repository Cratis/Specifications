// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_Reporting;

internal static class rendered_tree
{
    public static string Render(SpecificationReportOptions options) =>
        approved_snapshot.Join(Render(options, reporting_fixture.Reports()));

    static IEnumerable<OutputLine> Render(SpecificationReportOptions options, ClassReport[] reports)
    {
        var renderer = new ConsoleTreeRenderer(options);
        return reports.SelectMany(renderer.Render);
    }
}
