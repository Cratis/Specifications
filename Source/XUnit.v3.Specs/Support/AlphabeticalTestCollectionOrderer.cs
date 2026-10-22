// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit.Sdk;
using Xunit.v3;

namespace Cratis.Specifications.Support;

/// <summary>
/// Orders test collections alphabetically by display name so the specs in this assembly run in a
/// deterministic order.
/// </summary>
public class AlphabeticalTestCollectionOrderer : ITestCollectionOrderer
{
    /// <inheritdoc/>
    public IReadOnlyCollection<TTestCollection> OrderTestCollections<TTestCollection>(IReadOnlyCollection<TTestCollection> testCollections)
        where TTestCollection : ITestCollection
        => [.. testCollections.OrderBy(_ => _.TestCollectionDisplayName, StringComparer.Ordinal)];
}
