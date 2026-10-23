// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications;

/// <summary>
/// Defines an observer of the specification lifecycle, registered through <see cref="SpecificationObservers.Add"/>.
/// </summary>
/// <remarks>
/// Every adapter reports the same sequence for each specification instance it runs the lifecycle for:
/// <see cref="ScopeStarting"/>, then <see cref="PhaseCompleted"/> for Establish and Because (Because is not run when
/// Establish fails) and Destroy, then <see cref="ScopeEnded"/>. An exception thrown by an observer surfaces as a
/// failure of the lifecycle step it was called from.
/// </remarks>
internal interface ISpecificationObserver
{
    /// <summary>
    /// Called before Establish runs for a specification instance.
    /// </summary>
    /// <param name="scope">The scope of the specification instance.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask ScopeStarting(SpecificationScope scope);

    /// <summary>
    /// Called when a phase of a specification instance has completed, successfully or not.
    /// </summary>
    /// <param name="scope">The scope of the specification instance.</param>
    /// <param name="phase">The phase that completed.</param>
    /// <param name="startTimestamp">The <see cref="System.Diagnostics.Stopwatch"/> timestamp when the phase started.</param>
    /// <param name="endTimestamp">The <see cref="System.Diagnostics.Stopwatch"/> timestamp when the phase ended.</param>
    /// <param name="failure">The exception the phase failed with, if any.</param>
    void PhaseCompleted(SpecificationScope scope, SpecificationPhase phase, long startTimestamp, long endTimestamp, Exception? failure);

    /// <summary>
    /// Called after Destroy (and disposal, where the adapter owns it) for a specification instance.
    /// </summary>
    /// <param name="scope">The scope of the specification instance.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask ScopeEnded(SpecificationScope scope);
}
