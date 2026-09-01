// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit.Abstractions;
using Xunit.Sdk;

namespace Cratis.Specifications;

/// <summary>
/// The test collection runner for <see cref="SpecificationTestFramework"/>, running each test
/// class through <see cref="SpecificationTestClassRunner"/>.
/// </summary>
/// <param name="testCollection">The test collection that contains the tests to be run.</param>
/// <param name="testCases">The test cases to be run.</param>
/// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages.</param>
/// <param name="messageBus">The message bus to report run status to.</param>
/// <param name="testCaseOrderer">The test case orderer that will be used to decide how to order the tests.</param>
/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
public class SpecificationTestCollectionRunner(
    ITestCollection testCollection,
    IEnumerable<IXunitTestCase> testCases,
    IMessageSink diagnosticMessageSink,
    IMessageBus messageBus,
    ITestCaseOrderer testCaseOrderer,
    ExceptionAggregator aggregator,
    CancellationTokenSource cancellationTokenSource) : XunitTestCollectionRunner(testCollection, testCases, diagnosticMessageSink, messageBus, testCaseOrderer, aggregator, cancellationTokenSource)
{
    /// <inheritdoc/>
    protected override Task<RunSummary> RunTestClassAsync(ITestClass testClass, IReflectionTypeInfo @class, IEnumerable<IXunitTestCase> testCases)
        => new SpecificationTestClassRunner(
            testClass,
            @class,
            testCases,
            DiagnosticMessageSink,
            MessageBus,
            TestCaseOrderer,
            new ExceptionAggregator(Aggregator),
            CancellationTokenSource,
            CollectionFixtureMappings).RunAsync();
}
