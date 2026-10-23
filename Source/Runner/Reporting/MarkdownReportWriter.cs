// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;

namespace Cratis.Specifications.Runner;

/// <summary>
/// Writes the specification tree as a Markdown living documentation page.
/// </summary>
internal static class MarkdownReportWriter
{
    /// <summary>
    /// Renders the tree.
    /// </summary>
    /// <param name="tree">The tree.</param>
    /// <returns>The Markdown.</returns>
    public static string Render(ReportTree tree)
    {
        var builder = new StringBuilder();
        builder.Append("# Specifications\n\n");
        builder.Append($"{tree.Passed} passed, {tree.Failed} failed, {tree.Skipped} skipped in {DurationFormat.Format(tree.Duration)}\n\n");
        foreach (var node in tree.Roots.OrderBy(_ => _.Name, StringComparer.Ordinal)) Append(builder, node, 0);
        return builder.ToString();
    }

    static void Append(StringBuilder builder, ReportNode node, int depth)
    {
        var indent = new string(' ', depth * 2);
        if (node.IsFact)
        {
            var marker = node.Outcome switch { FactOutcome.Passed => "\u2705", FactOutcome.Failed => "\u274C", _ => "\u23ED\uFE0F" };
            builder.Append($"{indent}- {marker} {node.Name}");
            if (node.Outcome != FactOutcome.Skipped) builder.Append($" ({DurationFormat.Format(node.Duration)})");
            if (node.Outcome == FactOutcome.Skipped && !string.IsNullOrEmpty(node.SkipReason)) builder.Append($" (skipped: {node.SkipReason})");
            builder.Append('\n');
            if (node.Failure is not null) AppendFailure(builder, node.Failure, indent + "  ");
            return;
        }

        builder.Append($"{indent}- **{node.Name}**\n");
        if (node.ContextFailure is not null)
        {
            builder.Append($"{indent}  - \u274C The context failed\n");
            AppendFailure(builder, node.ContextFailure, indent + "    ");
        }

        foreach (var child in node.OrderedChildren()) Append(builder, child, depth + 1);
    }

    static void AppendFailure(StringBuilder builder, FailureInfo failure, string indent)
    {
        if (failure.Expected is not null && failure.Actual is not null)
        {
            builder.Append($"{indent}- Expected: `{failure.Expected}`\n");
            builder.Append($"{indent}- Actual: `{failure.Actual}`\n");
            return;
        }

        builder.Append($"{indent}- {failure.Message.Replace("\r", string.Empty).Replace("\n", " ")}\n");
    }
}
