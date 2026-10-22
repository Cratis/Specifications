// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_Specification.probes;

public class failing_establish_specification : Specification
{
    public int EstablishCalls { get; private set; }

    void Establish()
    {
        EstablishCalls++;
        throw new InvalidOperationException("Establish failed");
    }
}
