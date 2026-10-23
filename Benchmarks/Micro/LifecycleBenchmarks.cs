// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using BenchmarkDotNet.Attributes;

namespace Cratis.Specifications.Benchmarks;

[MemoryDiagnoser]
public class LifecycleBenchmarks
{
    Func<object, Type, Type, Task> _establish = null!;
    Action _clearCache = null!;
    object _instance = null!;
    Type _type = null!;

    [Params(1, 3)]
    public int Depth { get; set; }

    [Params(false, true)]
    public bool Async { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var lifecycle = typeof(SpecificationCancelledException).Assembly.GetType("Cratis.Specifications.SpecificationLifecycle", true)!;
        _establish = lifecycle.GetMethod("Establish")!.CreateDelegate<Func<object, Type, Type, Task>>();
        var cache = lifecycle.GetField("_cache", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;
        _clearCache = cache.GetType().GetMethod("Clear")!.CreateDelegate<Action>(cache);
        _instance = (Async, Depth) switch
        {
            (false, 1) => new SyncOne(),
            (false, _) => new SyncThree(),
            (true, 1) => new AsyncOne(),
            _ => new AsyncThree()
        };
        _type = _instance.GetType();
        _establish(_instance, _type, typeof(object)).GetAwaiter().GetResult();
    }

    // BDN uses one invocation per iteration with IterationSetup; cache clearing is not timed.
    [IterationSetup(Target = nameof(ColdDiscovery))]
    public void ClearCache() => _clearCache();

    [Benchmark]
    public Task ColdDiscovery() => _establish(_instance, _type, typeof(object));

    [Benchmark]
    public Task WarmInvoke() => _establish(_instance, _type, typeof(object));

    class SyncOne
    {
        public int Calls { get; protected set; }
        void Establish() => Calls++;
    }

    class SyncTwo : SyncOne
    {
        void Establish() => Calls++;
    }

    sealed class SyncThree : SyncTwo
    {
        void Establish() => Calls++;
    }

    class AsyncOne
    {
        public int Calls { get; protected set; }
        async Task Establish() { await Task.Yield(); Calls++; }
    }

    class AsyncTwo : AsyncOne
    {
        async Task Establish() { await Task.Yield(); Calls++; }
    }

    sealed class AsyncThree : AsyncTwo
    {
        async Task Establish() { await Task.Yield(); Calls++; }
    }
}
