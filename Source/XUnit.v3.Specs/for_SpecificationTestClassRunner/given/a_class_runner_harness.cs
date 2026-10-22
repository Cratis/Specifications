// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using Cratis.Specifications.Support;
using Xunit.Sdk;
using Xunit.v3;

namespace Cratis.Specifications.for_SpecificationTestClassRunner.given;

public class a_class_runner_harness : Specification
{
    protected SpyMessageBus message_bus;
    protected RunSummary result;

    void Establish() => message_bus = new SpyMessageBus();

    protected string DisplayNameOf(ITestResultMessage message) =>
        message_bus.Messages.OfType<ITestStarting>().Single(_ => _.TestUniqueID == message.TestUniqueID).TestDisplayName;

    protected async Task RunClass(Type type)
    {
        var testAssembly = new XunitTestAssembly(type.Assembly, configFilePath: null);
        var testCollection = new XunitTestCollection(testAssembly, null, false, "Harness test collection");
        var testClass = new XunitTestClass(type, testCollection);

        var testCases = type
            .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(_ => _.GetCustomAttributes<FactAttribute>().Any())
            .Select(_ => (IXunitTestCase)new XunitTestCase(
                new XunitTestMethod(testClass, _, []),
                $"{type.Name}.{_.Name}",
                $"{type.FullName}.{_.Name}",
                false))
            .ToArray();

        using var cancellationTokenSource = new CancellationTokenSource();
        await using var scheduler = ExecutionScheduler.CreateUnlimited();
        await using var collectionFixtures = new FixtureMappingManager("Collection");
        result = await new SpecificationTestClassRunner().Run(
            testClass,
            testCases,
            ExplicitOption.Off,
            message_bus,
            new ExceptionAggregator(),
            cancellationTokenSource,
            ParallelMode.None,
            scheduler,
            collectionFixtures);
    }
}
