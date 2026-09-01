// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit.Abstractions;
using Xunit.Sdk;

namespace Cratis.Specifications;

/// <summary>
/// The test method runner used for the shared specification lifecycle. Runs each test case
/// through <see cref="SpecificationTestCaseRunner"/> so every fact executes against the shared
/// <see cref="Specification"/> instance.
/// </summary>
/// <param name="sharedInstance">The shared <see cref="Specification"/> instance every fact runs against.</param>
/// <param name="testMethod">The test method to be run.</param>
/// <param name="class">The test class that contains the test method.</param>
/// <param name="method">The test method that contains the tests to be run.</param>
/// <param name="testCases">The test cases to be run.</param>
/// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages.</param>
/// <param name="messageBus">The message bus to report run status to.</param>
/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
/// <param name="constructorArguments">The constructor arguments for the test class.</param>
public class SpecificationTestMethodRunner(
    Specification sharedInstance,
    ITestMethod testMethod,
    IReflectionTypeInfo @class,
    IReflectionMethodInfo method,
    IEnumerable<IXunitTestCase> testCases,
    IMessageSink diagnosticMessageSink,
    IMessageBus messageBus,
    ExceptionAggregator aggregator,
    CancellationTokenSource cancellationTokenSource,
    object[] constructorArguments) : XunitTestMethodRunner(testMethod, @class, method, testCases, diagnosticMessageSink, messageBus, aggregator, cancellationTokenSource, constructorArguments)
{
    readonly object[] _constructorArguments = constructorArguments;

    /// <inheritdoc/>
    protected override Task<RunSummary> RunTestCaseAsync(IXunitTestCase testCase)
        => new SpecificationTestCaseRunner(
            sharedInstance,
            testCase,
            testCase.DisplayName,
            testCase.SkipReason,
            _constructorArguments,
            testCase.TestMethodArguments,
            MessageBus,
            new ExceptionAggregator(Aggregator),
            CancellationTokenSource).RunAsync();
}
