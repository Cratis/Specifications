// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Resources;

/// <summary>
/// Records what the probe resources and their leases did, and how many leases were held at once.
/// </summary>
internal sealed class ResourceLog
{
    readonly List<string> _entries = [];
    int _active;

    public IReadOnlyList<string> Entries
    {
        get
        {
            lock (_entries) return [.. _entries];
        }
    }

    public int Created { get; private set; }
    public int Starts { get; private set; }
    public int Disposals { get; private set; }
    public int Peak { get; private set; }

    public void Add(string entry)
    {
        lock (_entries) _entries.Add(entry);
    }

    public void ResourceCreated()
    {
        lock (_entries) Created++;
    }

    public void ResourceStarted()
    {
        lock (_entries) Starts++;
    }

    public void ResourceDisposed()
    {
        lock (_entries) Disposals++;
    }

    public void LeaseTaken()
    {
        lock (_entries) Peak = Math.Max(Peak, ++_active);
    }

    public void LeaseReleased()
    {
        lock (_entries) _active--;
    }
}
