// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_Specification.given;

public class a_context_logging_its_lifecycle : Specification
{
    // Shared by the fixture and the follow-up fixture that verifies Destroy afterwards.
    public static readonly List<string> Calls = [];

    void Establish() => Calls.Add("base.Establish");

    void Because() => Calls.Add("base.Because");

    void Destroy() => Calls.Add("base.Destroy");
}
