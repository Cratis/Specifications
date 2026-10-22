// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_Specification.probes;

public class derived_recording_specification : recording_specification
{
    void Establish() => Calls.Add("Establish derived");
}
