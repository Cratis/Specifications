// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner;

/// <summary>
/// Represents the options controlling how specifications are run.
/// </summary>
/// <param name="MaxParallelClasses">The maximum number of specification classes running at the same time.</param>
/// <param name="SequentialFacts">Whether the facts of a class are awaited one by one instead of running concurrently.</param>
/// <param name="FailFast">Whether to stop starting new classes after the first failure.</param>
/// <param name="LastFailedFirst">Whether classes that failed in the previous run are started first.</param>
/// <param name="Shard">The shard of the classes to run, if the run is sharded.</param>
/// <param name="CachePath">The path of the run cache file holding durations and failures; null when disabled.</param>
internal sealed record SpecificationRunOptions(
    int MaxParallelClasses,
    bool SequentialFacts,
    bool FailFast,
    bool LastFailedFirst,
    Shard? Shard,
    string? CachePath)
{
    /// <summary>
    /// Gets the default options.
    /// </summary>
    public static SpecificationRunOptions Default { get; } = new(Environment.ProcessorCount, false, false, false, null, null);
}
