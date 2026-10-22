// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_Specification.probes;

public class recording_specification : Specification
{
    public List<string> Calls { get; } = [];

    void Establish() => Calls.Add("Establish");

    async Task Because()
    {
        await Task.Yield();
        Calls.Add("Because");
    }

    void Destroy() => Calls.Add("Destroy");
}
