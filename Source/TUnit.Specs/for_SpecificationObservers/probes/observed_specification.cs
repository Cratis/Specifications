// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.Observation;

namespace Cratis.Specifications.for_SpecificationObservers.probes;

/// <summary>
/// A specification with the observation catalog's lifecycle. Derive one probe per case, as the shared instance is cached per class.
/// </summary>
public class observed_specification : Specification
{
    void Establish() => ObservedBehavior.Establish();

    void Because() => ObservedBehavior.Because();

    void Destroy() => ObservedBehavior.Destroy();
}
