// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using Cratis.Specifications.Support;
using Xunit.Sdk;

namespace Cratis.Specifications.for_SpecificationTestClassRunner.given;

public class a_class_runner_harness : Specification
{
    protected SpyMessageBus message_bus;
    protected RunSummary result;

    void Establish() => message_bus = new SpyMessageBus();

    protected async Task RunClass(Type type)
    {
        var diagnosticMessageSink = new NullDiagnosticMessageSink();
        var testAssembly = new TestAssembly(Reflector.Wrap(type.Assembly));
        var testCollection = new TestCollection(testAssembly, null, "Harness test collection");
        var typeInfo = Reflector.Wrap(type);
        var testClass = new TestClass(testCollection, typeInfo);

        var testCases = type
            .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(_ => _.GetCustomAttributes<FactAttribute>().Any())
            .Select(_ => (IXunitTestCase)new XunitTestCase(
                diagnosticMessageSink,
                TestMethodDisplay.ClassAndMethod,
                TestMethodDisplayOptions.None,
                new TestMethod(testClass, Reflector.Wrap(_))))
            .ToArray();

        using var cancellationTokenSource = new CancellationTokenSource();
        var runner = new SpecificationTestClassRunner(
            testClass,
            typeInfo,
            testCases,
            diagnosticMessageSink,
            message_bus,
            new DefaultTestCaseOrderer(diagnosticMessageSink),
            new ExceptionAggregator(),
            cancellationTokenSource,
            new Dictionary<Type, object>());

        result = await runner.RunAsync();
    }
}
