// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using BenchmarkDotNet.Attributes;
using Xunit;
using Xunit.Sdk;
#if XUNIT3
using Xunit.v3;
#else
using Xunit.Abstractions;
#endif

namespace Cratis.Specifications.Benchmarks;

[MemoryDiagnoser]
public class XUnitRunnerBenchmarks
{
    [Benchmark]
    public async Task<int> SharedClass()
    {
        var type = typeof(Context);
#if XUNIT3
        var assembly = new XunitTestAssembly(type.Assembly, configFilePath: null);
        var collection = new XunitTestCollection(assembly, null, false, "Benchmark collection");
        var testClass = new XunitTestClass(type, collection);
        var cases = type.GetMethods().Where(method => method.GetCustomAttributes<FactAttribute>().Any())
            .Select(method => (IXunitTestCase)new XunitTestCase(new XunitTestMethod(testClass, method, []), method.Name, method.Name, false)).ToArray();
        using var cancellation = new CancellationTokenSource();
        TestContext.SetForInitialization(null, false, false);
        TestContext.SetForTestAssembly(assembly, TestEngineStatus.Running, cancellation.Token);
        TestContext.SetForTestCollection(collection, TestEngineStatus.Running, cancellation.Token);
        await using var scheduler = ExecutionScheduler.CreateUnlimited();
        await using var fixtures = new FixtureMappingManager("Collection");
        using var bus = new NullBus();
        var summary = await new SpecificationTestClassRunner().Run(testClass, cases, ExplicitOption.Off, bus,
            new ExceptionAggregator(), cancellation, ParallelMode.None, scheduler, fixtures);
#else
        var sink = new NullSink();
        var assembly = new TestAssembly(Reflector.Wrap(type.Assembly));
        var collection = new TestCollection(assembly, null, "Benchmark collection");
        var typeInfo = Reflector.Wrap(type);
        var testClass = new TestClass(collection, typeInfo);
        var cases = type.GetMethods().Where(method => method.GetCustomAttributes<FactAttribute>().Any())
            .Select(method => (IXunitTestCase)new XunitTestCase(sink, TestMethodDisplay.ClassAndMethod, TestMethodDisplayOptions.None,
                new TestMethod(testClass, Reflector.Wrap(method)))).ToArray();
        using var cancellation = new CancellationTokenSource();
        using var bus = new NullBus();
        var runner = new SpecificationTestClassRunner(testClass, typeInfo, cases, sink, bus,
            new DefaultTestCaseOrderer(sink), new ExceptionAggregator(), cancellation, new Dictionary<Type, object>());
        var summary = await runner.RunAsync();
#endif
        if (summary is not { Total: 5, Failed: 0, Skipped: 0 }) throw new InvalidOperationException($"Expected five passing shared-context facts; total={summary.Total}, failed={summary.Failed}, skipped={summary.Skipped}: {bus.Failure}");
        return summary.Total;
    }

    sealed class NullBus : IMessageBus
    {
        public string? Failure { get; private set; }
        public bool QueueMessage(IMessageSinkMessage message)
        {
            if (message is ITestFailed failure) Failure = string.Join("; ", failure.Messages);
            if (message is IErrorMessage error) Failure = string.Join("; ", error.Messages);
            return true;
        }
        public void Dispose() { }
    }

#if !XUNIT3
    sealed class NullSink : LongLivedMarshalByRefObject, IMessageSink
    {
        public bool OnMessage(IMessageSinkMessage message) => true;
    }
#endif
}

public class Context : Specification
{
    int _value;
    void Establish() => _value = 1;
    void Because() => _value++;
    [Fact] public void should_one() => _value.ShouldEqual(2);
    [Fact] public void should_two() => _value.ShouldEqual(2);
    [Fact] public void should_three() => _value.ShouldEqual(2);
    [Fact] public void should_four() => _value.ShouldEqual(2);
    [Fact] public void should_five() => _value.ShouldEqual(2);
}
