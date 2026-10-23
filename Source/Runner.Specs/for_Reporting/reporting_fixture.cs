// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_Reporting;

/// <summary>
/// Deterministic classes with passed, failed and skipped facts, a context failure and phases.
/// </summary>
internal static class reporting_fixture
{
    static readonly DateTimeOffset _started = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    public static IReadOnlyList<(SpecificationWork Work, FactResult[] Results)> Classes()
    {
        var phases = new[]
        {
            new PhaseRecord(SpecificationPhase.Establish, PhaseRecord.SpecOwner, "Account", null, _started, TimeSpan.FromMilliseconds(3), false),
            new PhaseRecord(SpecificationPhase.Because, PhaseRecord.SpecOwner, "Account", null, _started, TimeSpan.FromMilliseconds(1500), false)
        };

        var depositing = Class(
            "Account \u203A when depositing",
            ["should_increase_balance", "should_record_deposit", "should_notify"],
            [
                FactResult.Passed(_started, TimeSpan.FromMilliseconds(12)) with { Steps = phases },
                FactResult.Failed(new SpecificationAssertionException("Expected: 1\nActual:   2", "1", "2"), _started, TimeSpan.FromMilliseconds(5)),
                FactResult.Skipped("not yet", _started)
            ]);

        var contextFailure = new InvalidOperationException("The database was not available");
        var closing = Class(
            "Account \u203A when closing \u203A and the balance is positive",
            ["should_refund", "should_close"],
            [
                FactResult.Failed(contextFailure, _started, TimeSpan.Zero),
                FactResult.Failed(contextFailure, _started, TimeSpan.Zero)
            ]);

        var other = Class(
            "Customer \u203A when registering",
            ["should_be_created"],
            [FactResult.Passed(_started, TimeSpan.FromMilliseconds(800))]);

        return [depositing, closing, other];
    }

    public static ClassReport[] Reports() => [.. Classes().Select(_ => ClassReport.From(_.Work, _.Results))];

    public static ReportTree Tree()
    {
        var tree = new ReportTree();
        foreach (var report in Reports()) tree.Add(report);
        return tree;
    }

    static (SpecificationWork Work, FactResult[] Results) Class(string displayName, string[] methods, FactResult[] results)
    {
        var facts = methods
            .Select((method, index) => new FactDescriptor(method, displayName + SpecificationDisplayNames.Separator + SpecificationDisplayNames.Readable(method), "/src/Account.cs", 40 + index, _ => ValueTask.CompletedTask))
            .ToArray();
        var specification = new SpecificationClassDescriptor(displayName, "Specs", displayName, displayName, null, null, null, null, facts);
        return (new SpecificationWork(specification, facts), results);
    }
}
