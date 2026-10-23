// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.for_SpecificationTestFramework;

public class when_running_facts_on_an_async_disposable_specification : Specification, IAsyncDisposable
{
    internal static int dispose_count;
    internal static int destroy_count;
    int _establishCount;
    void Establish() => _establishCount++;
    void Destroy() => destroy_count++;
    ValueTask IAsyncDisposable.DisposeAsync() { dispose_count++; return ValueTask.CompletedTask; }
    [Fact] void should_run_establish_once() => _establishCount.ShouldEqual(1);
    [Fact] void should_not_dispose_between_facts() => dispose_count.ShouldEqual(0);
}
