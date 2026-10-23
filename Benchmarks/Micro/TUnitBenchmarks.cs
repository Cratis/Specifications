// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

extern alias tu;

using System.Reflection;
using BenchmarkDotNet.Attributes;
using Constructor = tu::Cratis.Specifications.SpecificationClassConstructor;

namespace Cratis.Specifications.Benchmarks;

[MemoryDiagnoser]
public class TUnitBenchmarks
{
    readonly Constructor _constructor = new();
    readonly Context _initialized = new();
    Func<Type, object?> _release = null!;

    [GlobalSetup]
    public void Setup()
    {
        _release = typeof(Constructor).GetMethod("Release", BindingFlags.Static | BindingFlags.NonPublic)!.CreateDelegate<Func<Type, object?>>();
        _initialized.InitializeAsync().GetAwaiter().GetResult();
    }

    [Benchmark]
    public async Task<object?> CreateRelease()
    {
        await _constructor.Create(typeof(Context), null!);
        return _release(typeof(Context));
    }

    [Benchmark]
    public Task InitializeNew() => new Context().InitializeAsync();

    [Benchmark]
    public Task InitializeWarmLock() => _initialized.InitializeAsync();

    sealed class Context : tu::Cratis.Specifications.Specification
    {
        public int Value { get; private set; }
        void Establish() => Value = 1;
        void Because() => Value++;
    }
}
