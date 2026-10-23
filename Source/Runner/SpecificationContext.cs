// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications;

/// <summary>
/// Provides the context of the specification class currently running.
/// </summary>
public static class SpecificationContext
{
    static readonly AsyncLocal<CancellationToken> _cancellationToken = new();

    /// <summary>
    /// Gets the run's cooperative cancellation token. Cancellation stops scheduling new classes; running classes
    /// still finish their facts, Destroy and disposal. Outside a running class this is <see cref="System.Threading.CancellationToken.None"/>.
    /// </summary>
    public static CancellationToken CancellationToken
    {
        get => _cancellationToken.Value;
        internal set => _cancellationToken.Value = value;
    }
}
