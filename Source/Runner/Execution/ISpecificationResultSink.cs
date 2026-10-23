// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner;

/// <summary>
/// Defines a receiver of specification results, decoupling execution from the reporting transport.
/// </summary>
internal interface ISpecificationResultSink
{
    /// <summary>
    /// Called when a specification class starts.
    /// </summary>
    /// <param name="work">The work that starts.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask Starting(SpecificationWork work);

    /// <summary>
    /// Called once a specification class has completed, including Destroy and disposal, with the result of every selected fact.
    /// </summary>
    /// <param name="work">The work that completed.</param>
    /// <param name="results">The results, in the order of <see cref="SpecificationWork.Facts"/>.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask Completed(SpecificationWork work, IReadOnlyList<FactResult> results);
}
