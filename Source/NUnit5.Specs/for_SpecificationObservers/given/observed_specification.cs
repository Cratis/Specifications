// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.Observation;

namespace Cratis.Specifications.for_SpecificationObservers.given;

/// <summary>
/// A specification with the observation catalog's lifecycle. Internal so it is not discovered as a test fixture.
/// </summary>
internal sealed class observed_specification : Specification
{
    void Establish() => ObservedBehavior.Establish();

    void Because() => ObservedBehavior.Because();

    void Destroy() => ObservedBehavior.Destroy();
}
