// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;

namespace Cratis.Specifications.Probes;

/// <summary>
/// Records the calls the runner makes on probe specifications, per probe.
/// </summary>
public static class CallLog
{
    static readonly ConcurrentDictionary<string, ConcurrentQueue<string>> _calls = new();

    public static void Record(string probe, string call) => _calls.GetOrAdd(probe, _ => new()).Enqueue(call);

    public static IReadOnlyList<string> For(string probe) => _calls.TryGetValue(probe, out var calls) ? [.. calls] : [];

    public static void Clear(string probe) => _calls.TryRemove(probe, out _);
}
