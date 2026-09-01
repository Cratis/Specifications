// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_SpecificationTestFramework;

// A Specification that implements IDisposable is not eligible for the shared lifecycle - the stock
// invoker disposes the test class after every fact, so it falls back to the per-fact lifecycle.
public class when_running_facts_on_a_disposable_specification : Specification, IDisposable
{
    internal static int establish_count;
    internal static int facts_run;

    void Establish() => establish_count++;

    public void Dispose() => GC.SuppressFinalize(this);

    [Fact]
    void should_run_establish_for_the_first_fact()
    {
        facts_run++;
        establish_count.ShouldEqual(facts_run);
    }

    [Fact]
    void should_run_establish_for_the_second_fact()
    {
        facts_run++;
        establish_count.ShouldEqual(facts_run);
    }
}
