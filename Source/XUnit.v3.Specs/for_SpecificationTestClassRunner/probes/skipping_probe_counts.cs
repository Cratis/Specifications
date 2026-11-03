// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_SpecificationTestClassRunner.probes;

/// <summary>
/// Counts how often the skipping probes are constructed and established.
/// </summary>
static class skipping_probe_counts
{
    internal static int instances_created;
    internal static int establish_count;

    internal static void Reset() => instances_created = establish_count = 0;
}
