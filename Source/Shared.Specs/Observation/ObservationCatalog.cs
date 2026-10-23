// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Observation;

/// <summary>
/// The lifecycle event sequences every adapter reports to an <see cref="ISpecificationObserver"/>, one per case.
/// </summary>
/// <remarks>
/// Every package's specs run the same cases through their adapter and compare against these sequences.
/// Cancellation inside a lifecycle method surfaces as a failure of that phase.
/// </remarks>
internal static class ObservationCatalog
{
    public const string Starting = "scope starting";
    public const string Ended = "scope ended";

    public static readonly string[] Passing = [Starting, "Establish", "Because", "Destroy", Ended];
    public static readonly string[] EstablishFails = [Starting, "Establish failed", "Destroy", Ended];
    public static readonly string[] BecauseFails = [Starting, "Establish", "Because failed", "Destroy", Ended];
    public static readonly string[] DestroyFails = [Starting, "Establish", "Because", "Destroy failed", Ended];
    public static readonly string[] Cancelled = BecauseFails;
    public static readonly string[] AllSkipped = [];

    /// <summary>
    /// Gets the sequence for a number of instances that each run the passing lifecycle one after the other.
    /// </summary>
    /// <param name="instances">The number of instances.</param>
    /// <returns>The sequence.</returns>
    public static string[] PassingTimes(int instances) => [.. Enumerable.Repeat(Passing, instances).SelectMany(_ => _)];
}
