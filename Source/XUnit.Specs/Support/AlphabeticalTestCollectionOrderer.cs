// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit.Abstractions;

namespace Cratis.Specifications.Support;

/// <summary>
/// Orders test collections alphabetically by display name so the specs in this assembly run in a
/// deterministic order.
/// </summary>
public class AlphabeticalTestCollectionOrderer : ITestCollectionOrderer
{
    /// <inheritdoc/>
    public IEnumerable<ITestCollection> OrderTestCollections(IEnumerable<ITestCollection> testCollections)
        => testCollections.OrderBy(_ => _.DisplayName, StringComparer.Ordinal);
}
