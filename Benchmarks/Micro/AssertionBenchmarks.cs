// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

extern alias v2;
extern alias v3;
extern alias n4;
extern alias n5;
extern alias tu;

using BenchmarkDotNet.Attributes;

namespace Cratis.Specifications.Benchmarks;

[MemoryDiagnoser]
public class AssertionBenchmarks
{
    int[] _actual = null!;
    int[] _expected = null!;
    Action<IEnumerable<int>, IEnumerable<int>> _ordered = null!;
    Action<IEnumerable<int>, IEnumerable<int>> _unordered = null!;
    Action<IEnumerable<int>, int> _contains = null!;

    [Params("XUnit2", "XUnit3", "NUnit4", "NUnit5", "TUnit", "Runner")]
    public string Flavor { get; set; } = "XUnit2";

    [Params(10, 1000)]
    public int Count { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _actual = [.. Enumerable.Range(0, Count)];
        _expected = [.. Enumerable.Range(0, Count)];
        (Action<IEnumerable<int>, IEnumerable<int>> Ordered, Action<IEnumerable<int>, IEnumerable<int>> Unordered, Action<IEnumerable<int>, int> Contains) operations = Flavor switch
        {
            "XUnit2" => (v2::Cratis.Specifications.ShouldCollectionExtensions.ShouldEqualInOrder<int>, v2::Cratis.Specifications.ShouldCollectionExtensions.ShouldContainOnly<int>, v2::Cratis.Specifications.ShouldCollectionExtensions.ShouldContain<int>),
            "XUnit3" => (v3::Cratis.Specifications.ShouldCollectionExtensions.ShouldEqualInOrder<int>, v3::Cratis.Specifications.ShouldCollectionExtensions.ShouldContainOnly<int>, v3::Cratis.Specifications.ShouldCollectionExtensions.ShouldContain<int>),
            "NUnit4" => (n4::Cratis.Specifications.ShouldCollectionExtensions.ShouldEqualInOrder<int>, n4::Cratis.Specifications.ShouldCollectionExtensions.ShouldContainOnly<int>, n4::Cratis.Specifications.ShouldCollectionExtensions.ShouldContain<int>),
            "NUnit5" => (n5::Cratis.Specifications.ShouldCollectionExtensions.ShouldEqualInOrder<int>, n5::Cratis.Specifications.ShouldCollectionExtensions.ShouldContainOnly<int>, n5::Cratis.Specifications.ShouldCollectionExtensions.ShouldContain<int>),
            "TUnit" => (tu::Cratis.Specifications.ShouldCollectionExtensions.ShouldEqualInOrder<int>, tu::Cratis.Specifications.ShouldCollectionExtensions.ShouldContainOnly<int>, tu::Cratis.Specifications.ShouldCollectionExtensions.ShouldContain<int>),
            _ => (ShouldCollectionExtensions.ShouldEqualInOrder<int>, ShouldCollectionExtensions.ShouldContainOnly<int>, ShouldCollectionExtensions.ShouldContain<int>)
        };
        (_ordered, _unordered, _contains) = operations;
    }

    [Benchmark]
    public void Ordered() => _ordered(_actual, _expected);

    [Benchmark]
    public void Unordered() => _unordered(_actual, _expected);

    [Benchmark]
    public void ContainsLast() => _contains(_actual, Count - 1);
}
