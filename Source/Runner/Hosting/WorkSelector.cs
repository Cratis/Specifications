// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Testing.Platform.Requests;

namespace Cratis.Specifications.Runner;

/// <summary>
/// Selects the facts a request asks for.
/// </summary>
internal static class WorkSelector
{
    /// <summary>
    /// Selects the facts matching a filter, grouped per specification class.
    /// </summary>
    /// <param name="classes">The registered specification classes.</param>
    /// <param name="filter">The filter of the request.</param>
    /// <returns>The selected work, ordered by unique identifier; classes without selected facts are left out.</returns>
    /// <exception cref="NotSupportedException">The filter is not supported.</exception>
    public static IReadOnlyList<SpecificationWork> Select(IReadOnlyList<SpecificationClassDescriptor> classes, ITestExecutionFilter? filter)
    {
        var uids = UidsOf(filter);
        var selected = new List<SpecificationWork>(classes.Count);
        foreach (var specification in classes)
        {
            if (uids is null || uids.Contains(specification.Uid))
            {
                if (specification.Facts.Count > 0) selected.Add(new(specification, specification.Facts));
                continue;
            }

            List<FactDescriptor>? facts = null;
            foreach (var fact in specification.Facts)
            {
                if (uids.Contains(fact.Uid)) (facts ??= []).Add(fact);
            }

            if (facts is not null) selected.Add(new(specification, facts));
        }

        return selected;
    }

    static HashSet<string>? UidsOf(ITestExecutionFilter? filter) => filter switch
    {
        null or NopFilter => null,
        TestNodeUidListFilter list => list.TestNodeUids.Select(_ => _.Value).ToHashSet(StringComparer.Ordinal),
        _ => throw new NotSupportedException($"The filter '{filter.GetType().Name}' is not supported by the Cratis specification runner; use --filter-uid.")
    };
}
