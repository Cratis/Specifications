// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime;
using Cratis.Specifications;
using Xunit;

namespace Consumer.for_RuntimeDefaults;

public class when_opting_out_of_runtime_defaults : Specification
{
    [Fact]
    public void should_keep_concurrent_garbage_collection() => GCSettings.LatencyMode.ShouldEqual(GCLatencyMode.Interactive);

    [Fact]
    public void should_respect_the_concurrent_gc_property()
    {
#if RUNTIME_OVERRIDES
        string.Equals(AppContext.GetData("System.GC.Concurrent")?.ToString(), "true", StringComparison.OrdinalIgnoreCase).ShouldBeTrue();
#else
        AppContext.GetData("System.GC.Concurrent").ShouldBeNull();
#endif
    }

    [Fact]
    public void should_respect_the_tiered_pgo_property()
    {
#if RUNTIME_OVERRIDES
        string.Equals(AppContext.GetData("System.Runtime.TieredPGO")?.ToString(), "true", StringComparison.OrdinalIgnoreCase).ShouldBeTrue();
#else
        AppContext.GetData("System.Runtime.TieredPGO").ShouldBeNull();
#endif
    }
}
