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
            var instance = (Specification)Activator.CreateInstance(ctxt.TestClass.Class)!;
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
            await ctxt.Aggregator.RunAsync(() => new ValueTask(_sharedInstance.DestroyAsync()));
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
        testCase.GetType() == typeof(XunitTestCase) &&
        ((XunitTestCase)testCase).TestMethodArguments.Length == 0 &&
        !testCase.TestMethod.Method.GetCustomAttributes<TheoryAttribute>().Any();

    static bool IsEligibleForSharedLifecycle(XunitTestClassRunnerContext ctxt)
    {
        var type = ctxt.TestClass.Class;
        if (!typeof(Specification).IsAssignableFrom(type)) return false;
        if (typeof(IDisposable).IsAssignableFrom(type)) return false;

        var constructors = type.GetTypeInfo()
            .DeclaredConstructors
            .Where(_ => !_.IsStatic && _.IsPublic)
            .ToArray();
        if (constructors.Length != 1 || constructors[0].GetParameters().Length != 0) return false;

        return ctxt.TestCases.All(IsPlainFact);
    }
}
