// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using BenchmarkDotNet.Attributes;
using Cratis.Specifications.Runner;

namespace Cratis.Specifications.Benchmarks;

[MemoryDiagnoser]
public class RunnerBenchmarks
{
    readonly Context _instance = new();
    readonly MethodInfo _method = ReflectionInvoker.Find(typeof(Context), nameof(Context.Fact));
    readonly FactResult[] _results = new FactResult[5];
    FactDescriptor[] _syncFacts = null!;
    FactDescriptor[] _asyncFacts = null!;
    SpecificationWork[] _plan = null!;
    readonly CountingSink _sink = new();

    [GlobalSetup]
    public void Setup()
    {
        _syncFacts = [.. Enumerable.Range(0, 5).Select(index => new FactDescriptor($"should_{index}", "fact", null, 0, static unit => { ((Context)unit).Fact(); return default; }))];
        _asyncFacts = [.. Enumerable.Range(0, 5).Select(index => new FactDescriptor($"should_{index}", "fact", null, 0, static _ => Yield()))];
        _plan = [.. Enumerable.Range(0, 5000).Select(index =>
        {
            var descriptor = new SpecificationClassDescriptor($"Class{index}", "Benchmarks", $"Class{index}", "class", static () => new Context(), null, null, null, [_syncFacts[0]]);
            return new SpecificationWork(descriptor, descriptor.Facts);
        })];
    }

    [Benchmark(Baseline = true)]
    public ValueTask GeneratedCall() { _instance.Fact(); return default; }

    [Benchmark]
    public ValueTask ReflectionCall() => ReflectionInvoker.Invoke(_method, _instance);

    [Benchmark]
    public Task SyncFactsParallel() => FactRunner.Run(_instance, _syncFacts, _results, false, DateTimeOffset.UnixEpoch);

    [Benchmark]
    public Task SyncFactsSequential() => FactRunner.Run(_instance, _syncFacts, _results, true, DateTimeOffset.UnixEpoch);

    [Benchmark]
    public Task AsyncFactsParallel() => FactRunner.Run(_instance, _asyncFacts, _results, false, DateTimeOffset.UnixEpoch);

    [Benchmark]
    public Task AsyncFactsSequential() => FactRunner.Run(_instance, _asyncFacts, _results, true, DateTimeOffset.UnixEpoch);

    // Report per class; one invocation always schedules 5,000 distinct class descriptors.
    [Benchmark(OperationsPerInvoke = 5000)]
    public async Task ScheduleClasses()
    {
        _sink.Reset();
        var notStarted = await SpecificationScheduler.Run(_plan, SpecificationRunOptions.Default, _sink, RunCache.Empty, CancellationToken.None);
        if (notStarted.Count != 0 || _sink.CompletedCount != 5000) throw new InvalidOperationException("Scheduler did not complete all 5000 classes.");
    }

    static async ValueTask Yield() => await Task.Yield();

    sealed class Context
    {
        public int Calls { get; private set; }
        public void Fact() => Calls++;
    }

    sealed class CountingSink : ISpecificationResultSink
    {
        int _completed;
        public int CompletedCount => _completed;
        public void Reset() => _completed = 0;
        public ValueTask Starting(SpecificationWork work) => default;
        public ValueTask Completed(SpecificationWork work, IReadOnlyList<FactResult> results)
        {
            if (results.Count != 1 || results[0].Outcome != FactOutcome.Passed) throw new InvalidOperationException("Fact did not pass.");
            Interlocked.Increment(ref _completed);
            return default;
        }
    }
}
