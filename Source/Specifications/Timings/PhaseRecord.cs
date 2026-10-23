// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications;

/// <summary>
/// Represents the timing of one phase of a specification.
/// </summary>
/// <remarks>
/// Holds only type and display names, resource keys and durations; never exception messages or resource values.
/// </remarks>
/// <param name="Phase">The phase.</param>
/// <param name="Owner">The scope that owns the phase: <c>session</c>, <c>key</c> or <c>spec</c>.</param>
/// <param name="Specification">The display name of the affected specification, if any.</param>
/// <param name="ResourceKey">The key of the shared resource the phase concerns, if any.</param>
/// <param name="StartedUtc">When the phase started.</param>
/// <param name="Duration">How long the phase took.</param>
/// <param name="Failed">Whether the phase failed.</param>
internal readonly record struct PhaseRecord(
    SpecificationPhase Phase,
    string Owner,
    string? Specification,
    string? ResourceKey,
    DateTimeOffset StartedUtc,
    TimeSpan Duration,
    bool Failed)
{
    /// <summary>The owner of a phase that belongs to a specification instance.</summary>
    public const string SpecOwner = "spec";

    /// <summary>The owner of a phase that belongs to a shared resource key or group.</summary>
    public const string KeyOwner = "key";

    /// <summary>The owner of a phase that belongs to the test session.</summary>
    public const string SessionOwner = "session";
}
