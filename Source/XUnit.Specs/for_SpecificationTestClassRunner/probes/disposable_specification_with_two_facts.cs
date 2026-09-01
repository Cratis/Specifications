// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_SpecificationTestClassRunner.probes;

// Internal so test discovery never finds it - it is only run through the harness.
sealed class disposable_specification_with_two_facts : Specification, IDisposable
{
    internal static int establish_count;
    internal static int dispose_count;
    internal static int instances_created;

    public disposable_specification_with_two_facts() => instances_created++;

    internal static void Reset() => establish_count = dispose_count = instances_created = 0;

    void Establish() => establish_count++;

    public void Dispose()
    {
        dispose_count++;
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void first_fact()
    {
    }

    [Fact]
    public void second_fact()
    {
    }
}
