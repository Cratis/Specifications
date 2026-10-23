// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Observation;

/// <summary>
/// The cases of the observation catalog, chosen by the probes before they are run.
/// </summary>
internal enum ObservedFailure
{
    None = 0,
    Establish = 1,
    Because = 2,
    Destroy = 3,
    Cancelled = 4
}
