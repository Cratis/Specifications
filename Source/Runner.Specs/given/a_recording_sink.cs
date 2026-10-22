// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.given;

/// <summary>
/// Collects the results the scheduler reports.
/// </summary>
internal sealed class a_recording_sink : ISpecificationResultSink
{
    public System.Collections.Concurrent.ConcurrentQueue<(SpecificationWork Work, IReadOnlyList<FactResult> Results)> Completed { get; } = new();

    public ValueTask Starting(SpecificationWork work) => ValueTask.CompletedTask;

    ValueTask ISpecificationResultSink.Completed(SpecificationWork work, IReadOnlyList<FactResult> results)
    {
        Completed.Enqueue((work, results));
        return ValueTask.CompletedTask;
    }
}
