// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner;

/// <summary>
/// Represents the outcome of one specification class: the levels of its name and its facts.
/// </summary>
/// <param name="Levels">The levels of the readable class name.</param>
/// <param name="Facts">The facts.</param>
/// <param name="ContextFailure">The failure of the context shared by the facts, reported once; null when the context worked.</param>
internal sealed record ClassReport(IReadOnlyList<string> Levels, IReadOnlyList<ReportNode> Facts, FailureInfo? ContextFailure)
{
    /// <summary>
    /// Builds the report of a completed class.
    /// </summary>
    /// <param name="work">The work that completed.</param>
    /// <param name="results">The results, in the order of the facts.</param>
    /// <returns>The report.</returns>
    public static ClassReport From(SpecificationWork work, IReadOnlyList<FactResult> results)
    {
        var shared = SharedFailure(results);
        var facts = new List<ReportNode>(results.Count);
        for (var index = 0; index < results.Count; index++)
        {
            var fact = work.Facts[index];
            var result = results[index];
            facts.Add(new ReportNode(FactName(work.Specification, fact), true)
            {
                Outcome = result.Outcome,
                Duration = result.Duration,
                Failure = result.Outcome == FactOutcome.Failed && shared is null ? FailureInfo.From(result.Exception!) : null,
                SkipReason = result.SkipReason,
                FilePath = fact.FilePath,
                Line = fact.Line,
                Phases = result.Steps
            });
        }

        var levels = work.Specification.DisplayName.Split(SpecificationDisplayNames.Separator);
        return new(levels, facts, shared is null ? null : FailureInfo.From(shared));
    }

    /// <summary>
    /// The fact name as generated: the display name of the fact without the display name of its class, so it follows the readable names opt-out.
    /// </summary>
    static string FactName(SpecificationClassDescriptor specification, FactDescriptor fact)
    {
        var prefix = specification.DisplayName + SpecificationDisplayNames.Separator;
        return fact.DisplayName.StartsWith(prefix, StringComparison.Ordinal) ? fact.DisplayName.Substring(prefix.Length) : fact.DisplayName;
    }

    static Exception? SharedFailure(IReadOnlyList<FactResult> results)
    {
        Exception? first = null;
        var failed = 0;
        foreach (var result in results)
        {
            if (result.Outcome != FactOutcome.Failed) continue;
            if (first is not null && !ReferenceEquals(first, result.Exception)) return null;
            first = result.Exception;
            failed++;
        }

        if (failed == 0) return null;
        return failed > 1 || results.Count(_ => _.Outcome == FactOutcome.Failed && _.Duration == TimeSpan.Zero) == 1 ? first : null;
    }
}
