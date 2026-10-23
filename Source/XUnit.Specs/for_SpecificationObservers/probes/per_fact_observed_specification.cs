// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_SpecificationObservers.probes;

[PerFactSpecificationLifecycle]
sealed class per_fact_observed_specification : Specification
{
    void Establish() { }

    void Because() { }

    void Destroy() { }

    [Fact] public void first_fact() { }

    [Fact] public void second_fact() { }
}
