// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Cratis.Specifications;

/// <summary>
/// The test assembly runner for <see cref="SpecificationTestFramework"/>, running each test
/// collection through <see cref="SpecificationTestCollectionRunner"/>.
/// </summary>
/// <param name="testAssembly">The assembly that contains the tests to be run.</param>
/// <param name="testCases">The test cases to be run.</param>
/// <param name="diagnosticMessageSink">The message sink to report diagnostic messages to.</param>
/// <param name="executionMessageSink">The message sink to report run status to.</param>
/// <param name="executionOptions">The user's requested execution options.</param>
/// <remarks>
/// Overriding <see cref="RunTestCollectionAsync"/> bypasses the private parallelism semaphore the
/// stock <see cref="XunitTestAssemblyRunner"/> acquires there for the conservative parallel
/// algorithm, so this runner recreates an equivalent semaphore from the same execution options and
/// assembly attributes to preserve stock scheduling behavior.
/// </remarks>
public class SpecificationTestAssemblyRunner(
    ITestAssembly testAssembly,
    IEnumerable<IXunitTestCase> testCases,
    IMessageSink diagnosticMessageSink,
    IMessageSink executionMessageSink,
    ITestFrameworkExecutionOptions executionOptions) : XunitTestAssemblyRunner(testAssembly, testCases, diagnosticMessageSink, executionMessageSink, executionOptions)
{
    SemaphoreSlim? _parallelSemaphore;
    bool _parallelSemaphoreInitialized;

    /// <inheritdoc/>
    public override void Dispose()
    {
        _parallelSemaphore?.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    protected override async Task<RunSummary> RunTestCollectionAsync(IMessageBus messageBus, ITestCollection testCollection, IEnumerable<IXunitTestCase> testCases, CancellationTokenSource cancellationTokenSource)
    {
        EnsureParallelSemaphore();

        if (_parallelSemaphore is not null)
        {
            await _parallelSemaphore.WaitAsync(cancellationTokenSource.Token);
        }

        try
        {
            return await new SpecificationTestCollectionRunner(
                testCollection,
                testCases,
                DiagnosticMessageSink,
                messageBus,
                TestCaseOrderer,
                new ExceptionAggregator(Aggregator),
                cancellationTokenSource).RunAsync();
        }
        finally
        {
            _parallelSemaphore?.Release();
        }
    }

    void EnsureParallelSemaphore()
    {
        if (_parallelSemaphoreInitialized) return;
        _parallelSemaphoreInitialized = true;

        var collectionBehavior = TestAssembly.Assembly.GetCustomAttributes(typeof(CollectionBehaviorAttribute)).SingleOrDefault();
        var disableParallelization = collectionBehavior?.GetNamedArgument<bool>(nameof(CollectionBehaviorAttribute.DisableTestParallelization)) ?? false;
        disableParallelization = ExecutionOptions.DisableParallelization() ?? disableParallelization;

        var maxParallelThreads = collectionBehavior?.GetNamedArgument<int>(nameof(CollectionBehaviorAttribute.MaxParallelThreads)) ?? 0;
        maxParallelThreads = ExecutionOptions.MaxParallelThreads() ?? maxParallelThreads;
        if (maxParallelThreads == 0)
        {
            maxParallelThreads = Environment.ProcessorCount;
        }

        if (!disableParallelization &&
            ExecutionOptions.ParallelAlgorithmOrDefault() == ParallelAlgorithm.Conservative &&
            maxParallelThreads > 0)
        {
            _parallelSemaphore = new SemaphoreSlim(maxParallelThreads);
        }
    }
}
