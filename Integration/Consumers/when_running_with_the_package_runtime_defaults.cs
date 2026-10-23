// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Specifications;
using System.Runtime;
#if NUNIT
using System.Reflection;
using NUnit.Framework;
#endif

namespace Consumer.for_RuntimeDefaults;

public class when_running_with_the_package_runtime_defaults : Specification
{
#if XUNIT
    [Xunit.Fact]
#elif NUNIT
    [Test]
#elif TUNIT
    [global::TUnit.Core.Test]
#endif
    public void should_disable_concurrent_garbage_collection() =>
        string.Equals(AppContext.GetData("System.GC.Concurrent")?.ToString(), "false", StringComparison.OrdinalIgnoreCase).ShouldBeTrue();

#if XUNIT
    [Xunit.Fact]
#elif NUNIT
    [Test]
#elif TUNIT
    [global::TUnit.Core.Test]
#endif
    public void should_use_batch_garbage_collection() => GCSettings.LatencyMode.ShouldEqual(GCLatencyMode.Batch);

#if XUNIT
    [Xunit.Fact]
#elif NUNIT
    [Test]
#elif TUNIT
    [global::TUnit.Core.Test]
#endif
    public void should_disable_tiered_pgo() =>
        string.Equals(AppContext.GetData("System.Runtime.TieredPGO")?.ToString(), "false", StringComparison.OrdinalIgnoreCase).ShouldBeTrue();

#if NUNIT
    [Test]
    public void should_respect_the_fixture_parallelism_switch()
    {
        var attribute = GetType().Assembly.GetCustomAttribute<ParallelizableAttribute>();
#if PARALLEL_FIXTURES
        attribute.ShouldNotBeNull();
        attribute.Scope.ShouldEqual(ParallelScope.Fixtures);
#else
        attribute.ShouldBeNull();
#endif
    }
#endif
}
