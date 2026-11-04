// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using Xunit;
using Xunit.v3;

namespace Cratis.Specifications;

/// <summary>
/// Represents a test class runner that, for an eligible <see cref="Specification"/> class, creates a single
/// instance, runs Establish and Because once, runs every fact against that instance and runs Destroy once
/// after the last fact. Any other class runs with the stock xUnit per-fact lifecycle.
/// </summary>
/// <remarks>
/// A new runner is created for every test class, so the shared instance lives exactly as long as the class run.
/// A failure in Establish or Because is recorded while the class is starting, which makes xUnit fail every
/// fact in the class with that exception without running any of them.
/// </remarks>
public class SpecificationTestClassRunner : XunitTestClassRunner
{
    Specification? _sharedInstance;

    /// <inheritdoc/>
    protected override async ValueTask<bool> OnTestClassStarting(XunitTestClassRunnerContext ctxt)
    {
        var result = await base.OnTestClassStarting(ctxt);
        if (ctxt.Aggregator.HasExceptions || !IsEligibleForSharedLifecycle(ctxt)) return result;

        await ctxt.Aggregator.RunAsync(async () =>
        {
            var arguments = await CreateTestClassConstructorArguments(ctxt);
            if (ctxt.Aggregator.HasExceptions) return;
            var instance = (Specification)Activator.CreateInstance(ctxt.TestClass.Class, arguments)!;
            instance.IsSharedInstance = true;
            _sharedInstance = instance;
            await instance.InitializeAsync();
        });

        return result;
    }

    /// <inheritdoc/>
    protected override async ValueTask<bool> OnTestClassFinished(XunitTestClassRunnerContext ctxt, RunSummary summary)
    {
        if (_sharedInstance is not null)
        {
            await ctxt.Aggregator.RunAsync(() => new ValueTask(DisposeSharedInstance(_sharedInstance)));
        }

        return await base.OnTestClassFinished(ctxt, summary);
    }

    /// <inheritdoc/>
    protected override ValueTask<RunSummary> RunTestMethod(
        XunitTestClassRunnerContext ctxt,
        IXunitTestMethod? testMethod,
        IReadOnlyCollection<IXunitTestCase> testCases)
        => _sharedInstance is null || testMethod is null
            ? base.RunTestMethod(ctxt, testMethod, testCases)
            : new SpecificationTestMethodRunner(_sharedInstance).Run(
                testMethod,
                testCases,
                ctxt.ExplicitOption,
                ctxt.MessageBus,
                ctxt.Aggregator.Clone(),
                ctxt.CancellationTokenSource,
                ctxt.ParallelMode,
                ctxt.Scheduler,
                ctxt.ConstructorArguments ?? [],
                ctxt.ClassFixtureMappings);

    static bool IsPlainFact(IXunitTestCase testCase) =>
        (testCase.GetType() == typeof(XunitTestCase) || testCase.GetType() == typeof(SpecificationTestCase)) &&
        ((XunitTestCase)testCase).TestMethodArguments.Length == 0 &&
        !testCase.TestMethod.Method.GetCustomAttributes<TheoryAttribute>().Any();

    static bool IsEligibleForSharedLifecycle(XunitTestClassRunnerContext ctxt)
    {
        var type = ctxt.TestClass.Class;
        if (!typeof(Specification).IsAssignableFrom(type)) return false;
        if (type.Assembly.IsDefined(typeof(PerFactSpecificationLifecycleAttribute))) return false;
        if (HasCustomAsyncLifetime(type)) return false;

        var constructors = type.GetTypeInfo()
            .DeclaredConstructors
            .Where(_ => !_.IsStatic && _.IsPublic)
            .ToArray();
        if (constructors.Length != 1) return false;

        var fixtures = ctxt.TestClass.ClassFixtureTypes
            .Concat(ctxt.TestClass.TestCollection.CollectionFixtureTypes)
            .Concat(ctxt.TestClass.TestCollection.TestAssembly.AssemblyFixtureTypes).ToArray();
        if (!constructors[0].GetParameters().All(parameter => IsFixture(parameter.ParameterType, fixtures))) return false;
        return ctxt.TestCases.All(IsPlainFact);
    }

    static bool IsFixture(Type type, Type[] fixtures)
    {
        if (fixtures.Contains(type)) return true;
        if (!type.IsConstructedGenericType) return false;
        return fixtures.Contains(type.GetGenericTypeDefinition());
    }

    static bool HasCustomAsyncLifetime(Type type) =>
        type.GetInterfaceMap(typeof(IAsyncLifetime)).TargetMethods
            .Any(method => method.Name.Contains(nameof(IAsyncLifetime.InitializeAsync), StringComparison.Ordinal) && method.DeclaringType != typeof(Specification));

    static bool HasCustomAsyncDisposal(Type type) =>
        type.GetInterfaceMap(typeof(IAsyncDisposable)).TargetMethods.Any(method => method.DeclaringType != typeof(Specification));

    static async Task DisposeSharedInstance(Specification instance)
    {
        try
        {
            await instance.DestroyAsync();
        }
        finally
        {
            if (HasCustomAsyncDisposal(instance.GetType()) && instance is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
            else
            {
                (instance as IDisposable)?.Dispose();
            }
        }
    }
}
