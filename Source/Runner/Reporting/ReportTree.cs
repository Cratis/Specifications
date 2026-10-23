// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner;

/// <summary>
/// Collects the completed classes into the tree of the reports.
/// </summary>
internal sealed class ReportTree
{
    readonly Lock _lock = new();
    readonly ReportNode _root = new(string.Empty, false);

    /// <summary>
    /// Gets the top level nodes.
    /// </summary>
    public IReadOnlyList<ReportNode> Roots => _root.Children;

    /// <summary>
    /// Gets the number of passed facts.
    /// </summary>
    public int Passed => Count(FactOutcome.Passed);

    /// <summary>
    /// Gets the number of failed facts.
    /// </summary>
    public int Failed => Count(FactOutcome.Failed);

    /// <summary>
    /// Gets the number of skipped facts.
    /// </summary>
    public int Skipped => Count(FactOutcome.Skipped);

    /// <summary>
    /// Gets the summed duration of the facts.
    /// </summary>
    public TimeSpan Duration => Facts(_root).Aggregate(TimeSpan.Zero, (total, fact) => total + fact.Duration);

    /// <summary>
    /// Adds a completed class.
    /// </summary>
    /// <param name="report">The class report.</param>
    public void Add(ClassReport report)
    {
        lock (_lock)
        {
            var node = _root;
            foreach (var level in report.Levels)
            {
                var child = node.Children.Find(_ => !_.IsFact && _.Name == level);
                if (child is null)
                {
                    child = new ReportNode(level, false);
                    node.Children.Add(child);
                }

                node = child;
            }

            node.ContextFailure ??= report.ContextFailure;
            node.Children.AddRange(report.Facts);
        }
    }

    /// <summary>
    /// Enumerates the facts below a node.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns>The facts.</returns>
    public static IEnumerable<ReportNode> Facts(ReportNode node) =>
        node.IsFact ? [node] : node.Children.SelectMany(Facts);

    int Count(FactOutcome outcome) => Facts(_root).Count(_ => _.Outcome == outcome);
}
