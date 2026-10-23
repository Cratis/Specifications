// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Cratis.Specifications;

/// <summary>
/// The test class runner for <see cref="SpecificationTestFramework"/>. For eligible
/// <see cref="Specification"/> classes it constructs the test class once, runs Establish and
/// Because once, runs every fact against that shared instance and runs Destroy once after the
/// last fact. Everything else runs through the stock <see cref="XunitTestClassRunner"/> behavior.
/// </summary>
/// <param name="testClass">The test class to be run.</param>
/// <param name="class">The test class that contains the tests to be run.</param>
/// <param name="testCases">The test cases to be run.</param>
/// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages.</param>
/// <param name="messageBus">The message bus to report run status to.</param>
/// <param name="testCaseOrderer">The test case orderer that will be used to decide how to order the tests.</param>
/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
/// <param name="collectionFixtureMappings">The mapping of collection fixture types to fixtures.</param>
/// <remarks>
/// A class is eligible for the shared lifecycle when it derives from <see cref="Specification"/>,
/// has a single public parameterless constructor, does not implement <see cref="IDisposable"/>
/// (the stock invoker would dispose the shared instance after every fact) and all its test cases
/// are plain facts. The shared lifecycle mirrors class fixture semantics: a failing Establish or
/// Because is added to the <see cref="TestClassRunner{TTestCase}.Aggregator"/> so every fact in
/// the class fails with that exception, and Destroy runs at class cleanup even when
/// initialization failed, with failures reported as test class cleanup failures.
/// </remarks>
public class SpecificationTestClassRunner(
    ITestClass testClass,
    IReflectionTypeInfo @class,
    IEnumerable<IXunitTestCase> testCases,
    IMessageSink diagnosticMessageSink,
    IMessageBus messageBus,
    ITestCaseOrderer testCaseOrderer,
    ExceptionAggregator aggregator,
    CancellationTokenSource cancellationTokenSource,
    IDictionary<Type, object> collectionFixtureMappings) : XunitTestClassRunner(testClass, @class, testCases, diagnosticMessageSink, messageBus, testCaseOrderer, aggregator, cancellationTokenSource, collectionFixtureMappings)
{
    readonly IDictionary<Type, object> _collectionFixtureMappings = collectionFixtureMappings;
    Specification? _sharedInstance;

    /// <inheritdoc/>
    protected override async Task AfterTestClassStartingAsync()
    {
        await base.AfterTestClassStartingAsync();

        if (!IsEligibleForSharedLifecycle()) return;

        await Aggregator.RunAsync(async () =>
        {
            var arguments = CreateTestClassConstructorArguments();
            if (Aggregator.HasExceptions) return;
            var instance = (Specification)Activator.CreateInstance(Class.Type, arguments)!;
            instance.IsSharedInstance = true;
            _sharedInstance = instance;
            await instance.InitializeAsync();
        });
    }

    /// <inheritdoc/>
    protected override async Task BeforeTestClassFinishedAsync()
    {
        if (_sharedInstance is not null)
        {
            await Aggregator.RunAsync(() => DisposeSharedInstance(_sharedInstance));
        }

        await base.BeforeTestClassFinishedAsync();
    }

    /// <inheritdoc/>
    protected override Task<RunSummary> RunTestMethodAsync(ITestMethod testMethod, IReflectionMethodInfo method, IEnumerable<IXunitTestCase> testCases, object[] constructorArguments)
        => _sharedInstance is null
            ? base.RunTestMethodAsync(testMethod, method, testCases, constructorArguments)
            : new SpecificationTestMethodRunner(
                _sharedInstance,
                testMethod,
                Class,
                method,
                testCases,
                DiagnosticMessageSink,
                MessageBus,
                new ExceptionAggregator(Aggregator),
                CancellationTokenSource,
                constructorArguments).RunAsync();

    static bool IsPlainFact(IXunitTestCase testCase) =>
        (testCase.GetType() == typeof(XunitTestCase) || testCase.GetType() == typeof(SpecificationTestCase)) &&
        (testCase.TestMethodArguments is null || testCase.TestMethodArguments.Length == 0) &&
        !testCase.TestMethod.Method.GetCustomAttributes(typeof(TheoryAttribute)).Any();

    bool IsEligibleForSharedLifecycle()
    {
        var type = Class.Type;
        if (!typeof(Specification).IsAssignableFrom(type)) return false;
        if (type.Assembly.IsDefined(typeof(PerFactSpecificationLifecycleAttribute))) return false;
        if (type.IsDefined(typeof(PerFactSpecificationLifecycleAttribute), inherit: true)) return false;
        if (HasCustomAsyncLifetime(type)) return false;

        var constructors = type.GetTypeInfo()
            .DeclaredConstructors
            .Where(_ => !_.IsStatic && _.IsPublic)
            .ToArray();
        if (constructors.Length != 1) return false;

        if (!constructors[0].GetParameters().All(parameter => IsFixture(parameter.ParameterType))) return false;
        return TestCases.All(IsPlainFact);
    }

    bool IsFixture(Type type) => ClassFixtureMappings.ContainsKey(type) || _collectionFixtureMappings.ContainsKey(type);

    static bool HasCustomAsyncLifetime(Type type) =>
        type.GetInterfaceMap(typeof(IAsyncLifetime)).TargetMethods
            .Any(method => method.DeclaringType != typeof(Specification));

    static async Task DisposeSharedInstance(Specification instance)
    {
        try
        {
            await instance.DestroyAsync();
        }
        finally
        {
            try
            {
                if (instance is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync();
                }
                else
                {
                    (instance as IDisposable)?.Dispose();
                }
            }
            finally
            {
                await instance.EndScope();
            }
        }
    }
}
