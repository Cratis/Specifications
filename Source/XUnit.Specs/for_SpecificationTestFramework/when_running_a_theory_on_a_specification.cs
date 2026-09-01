// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_SpecificationTestFramework;

// A class with a theory is not eligible for the shared lifecycle - it falls back to the stock
// per-fact lifecycle, running Establish for every data row.
public class when_running_a_theory_on_a_specification : Specification
{
    internal static int establish_count;
    internal static readonly HashSet<int> rows_run = [];

    void Establish() => establish_count++;

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    void should_run_establish_for_every_data_row(int row)
    {
        rows_run.Add(row);
        establish_count.ShouldEqual(rows_run.Count);
    }
}
