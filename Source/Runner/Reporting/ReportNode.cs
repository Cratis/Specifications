// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner;

/// <summary>
/// Describes why a fact or a specification context failed.
/// </summary>
/// <param name="Message">The failure message.</param>
/// <param name="Expected">The formatted expected value, when the assertion compared values.</param>
/// <param name="Actual">The formatted actual value, when the assertion compared values.</param>
/// <param name="Details">The exception type and stack trace.</param>
internal sealed record FailureInfo(string Message, string? Expected, string? Actual, string? Details)
{
    /// <summary>
    /// Creates the description of an exception.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <returns>The description.</returns>
    public static FailureInfo From(Exception exception)
    {
        var assertion = exception as SpecificationAssertionException;
        return new(exception.Message, assertion?.Expected, assertion?.Actual, exception.StackTrace is null ? null : $"{exception.GetType().FullName}{Environment.NewLine}{exception.StackTrace}");
    }
}

/// <summary>
/// Represents a node of the specification tree: a group (a level of the class name) or a fact.
/// </summary>
/// <param name="Name">The readable name.</param>
/// <param name="IsFact">Whether the node is a fact.</param>
internal sealed class ReportNode(string Name, bool IsFact)
{
    /// <summary>Gets the readable name.</summary>
    public string Name { get; } = Name;

    /// <summary>Gets a value indicating whether the node is a fact.</summary>
    public bool IsFact { get; } = IsFact;

    /// <summary>Gets or sets the outcome of a fact.</summary>
    public FactOutcome Outcome { get; init; }

    /// <summary>Gets or sets the duration of a fact.</summary>
    public TimeSpan Duration { get; init; }

    /// <summary>Gets or sets why a fact failed.</summary>
    public FailureInfo? Failure { get; init; }

    /// <summary>Gets or sets why a fact was skipped.</summary>
    public string? SkipReason { get; init; }

    /// <summary>Gets or sets the source file of a fact.</summary>
    public string? FilePath { get; init; }

    /// <summary>Gets or sets the source line of a fact.</summary>
    public int Line { get; init; }

    /// <summary>Gets or sets the phases of a fact.</summary>
    public IReadOnlyList<PhaseRecord>? Phases { get; init; }

    /// <summary>Gets or sets the failure of the context a group describes, reported once for all its facts.</summary>
    public FailureInfo? ContextFailure { get; set; }

    /// <summary>Gets the children of a group.</summary>
    public List<ReportNode> Children { get; } = [];

    /// <summary>
    /// Gets the children: groups ordered by name, then the facts in the order they are declared.
    /// </summary>
    /// <returns>The ordered children.</returns>
    public IEnumerable<ReportNode> OrderedChildren() => Children.Where(_ => !_.IsFact).OrderBy(_ => _.Name, StringComparer.Ordinal).Concat(Children.Where(_ => _.IsFact));
}
