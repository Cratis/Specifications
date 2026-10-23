// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Observation;

/// <summary>
/// Registers session-end callbacks that record that they ran, optionally failing.
/// </summary>
internal sealed class SessionRecorder
{
    readonly List<string> _ended = [];

    public IReadOnlyList<string> Ended
    {
        get
        {
            lock (_ended) return [.. _ended];
        }
    }

    public SessionRecorder Register(string name, Exception failure = null)
    {
        SpecificationSession.Register(() =>
        {
            lock (_ended) _ended.Add(name);
            return failure is null ? ValueTask.CompletedTask : ValueTask.FromException(failure);
        });
        return this;
    }
}
