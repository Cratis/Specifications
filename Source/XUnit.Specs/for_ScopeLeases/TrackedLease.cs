// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_ScopeLeases;

internal sealed class TrackedLease : IAsyncDisposable
{
    public int Releases { get; private set; }

    public ValueTask DisposeAsync()
    {
        Releases++;
        return ValueTask.CompletedTask;
    }
}
