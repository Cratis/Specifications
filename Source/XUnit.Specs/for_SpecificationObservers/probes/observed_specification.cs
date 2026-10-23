// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications.Observation;

namespace Cratis.Specifications.for_SpecificationObservers.probes;

sealed class observed_specification : Specification
{
    void Establish() => ObservedBehavior.Establish();

    void Because() => ObservedBehavior.Because();

    void Destroy() => ObservedBehavior.Destroy();

    [Fact] public void first_fact() { }

    [Fact] public void second_fact() { }
}
