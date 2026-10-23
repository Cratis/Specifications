// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_Specification.given;

/// <summary>
/// A specification that records its lifecycle calls. Internal so it is not discovered as a test fixture.
/// </summary>
internal sealed class a_lifecycle_probe : Specification
{
    public List<string> Calls { get; } = [];

    void Establish() => Calls.Add("Establish");

    void Because() => Calls.Add("Because");

    void Destroy() => Calls.Add("Destroy");
}
