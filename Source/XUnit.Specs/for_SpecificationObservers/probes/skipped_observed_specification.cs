// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_SpecificationObservers.probes;

#pragma warning disable xUnit1004 // The probe skips every fact on purpose.

sealed class skipped_observed_specification : Specification
{
    void Establish() { }

    [Fact(Skip = "Probe")] public void first_fact() { }

    [Fact(Skip = "Probe")] public void second_fact() { }
}
