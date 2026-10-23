// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_SpecificationTestClassRunner.probes;

[PerFactSpecificationLifecycle]
sealed class per_fact_specification_with_two_facts : Specification
{
    internal static int instances_created;
    internal static int establish_count;
    internal static int because_count;
    internal static int destroy_count;

    public per_fact_specification_with_two_facts() => instances_created++;

    internal static void Reset() => instances_created = establish_count = because_count = destroy_count = 0;

    void Establish() => establish_count++;

    void Because() => because_count++;

    void Destroy() => destroy_count++;

    [Fact] public void first_fact() { }

    [Fact] public void second_fact() { }
}
