// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner;

/// <summary>
/// Renders completed classes as a tree grouped by the levels of their names. Classes complete in any order, so the levels shared
/// with the previously printed class are not repeated.
/// </summary>
/// <param name="options">The report options.</param>
internal sealed class ConsoleTreeRenderer(SpecificationReportOptions options)
{
    IReadOnlyList<string> _previous = [];

    /// <summary>
    /// Renders a class.
    /// </summary>
    /// <param name="report">The class report.</param>
    /// <returns>The lines to print.</returns>
    public IReadOnlyList<OutputLine> Render(ClassReport report)
    {
        var lines = new List<OutputLine>();
        var shared = 0;
        while (shared < _previous.Count && shared < report.Levels.Count - 1 && _previous[shared] == report.Levels[shared]) shared++;
        for (var depth = shared; depth < report.Levels.Count; depth++)
        {
            lines.Add(new(Indent(depth) + report.Levels[depth], LineStyle.Plain));
        }

        _previous = report.Levels;
        var factDepth = report.Levels.Count;
        if (report.ContextFailure is not null)
        {
            lines.Add(new($"{Indent(factDepth)}{Marker(FactOutcome.Failed)} The context failed", LineStyle.Failed));
            AddFailure(lines, report.ContextFailure, null, 0, factDepth + 1);
        }

        foreach (var fact in report.Facts) AddFact(lines, fact, factDepth);
        return lines;
    }

    static string Indent(int depth) => new(' ', depth * 2);

    static LineStyle StyleOf(FactOutcome outcome) => outcome switch
    {
        FactOutcome.Passed => LineStyle.Passed,
        FactOutcome.Failed => LineStyle.Failed,
        _ => LineStyle.Skipped
    };

    string Marker(FactOutcome outcome) => (options.Ascii, outcome) switch
    {
        (false, FactOutcome.Passed) => "\u2713",
        (false, FactOutcome.Failed) => "\u2717",
        (false, _) => "\u25CB",
        (true, FactOutcome.Passed) => "+",
        (true, FactOutcome.Failed) => "x",
        _ => "-"
    };

    void AddFact(List<OutputLine> lines, ReportNode fact, int depth)
    {
        var text = $"{Indent(depth)}{Marker(fact.Outcome)} {fact.Name}";
        if (options.Level == OutputLevel.Detailed && fact.Outcome != FactOutcome.Skipped) text += $" ({DurationFormat.Format(fact.Duration)})";
        if (fact.Outcome == FactOutcome.Skipped && !string.IsNullOrEmpty(fact.SkipReason)) text += $" (skipped: {fact.SkipReason})";
        lines.Add(new(text, StyleOf(fact.Outcome)));
        if (fact.Failure is not null) AddFailure(lines, fact.Failure, fact, fact.Line, depth + 2);
        AddPhases(lines, fact, depth + 2);
    }

    void AddFailure(List<OutputLine> lines, FailureInfo failure, ReportNode? fact, int line, int depth)
    {
        var indent = Indent(depth);
        if (failure.Expected is not null && failure.Actual is not null)
        {
            lines.Add(new($"{indent}Expected: {failure.Expected}", LineStyle.Failed));
            lines.Add(new($"{indent}Actual:   {failure.Actual}", LineStyle.Failed));
        }
        else
        {
            foreach (var messageLine in failure.Message.Split('\n')) lines.Add(new(indent + messageLine.TrimEnd('\r'), LineStyle.Failed));
        }

        if (fact?.FilePath is not null) lines.Add(new($"{indent}at {fact.FilePath}:{line}", LineStyle.Dim));
        if (options.Level == OutputLevel.Detailed && failure.Details is not null)
        {
            foreach (var detail in failure.Details.Split('\n')) lines.Add(new(indent + detail.TrimEnd('\r'), LineStyle.Dim));
        }
    }

    void AddPhases(List<OutputLine> lines, ReportNode fact, int depth)
    {
        if (fact.Phases is null) return;
        foreach (var phase in fact.Phases)
        {
            var slow = phase.Duration >= options.SlowThreshold;
            if (options.Level != OutputLevel.Detailed && !slow) continue;
            lines.Add(new($"{Indent(depth)}{phase.Phase} {DurationFormat.Format(phase.Duration)}{(slow ? " (slow)" : string.Empty)}", LineStyle.Dim));
        }
    }
}
