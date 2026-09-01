// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Cratis.Specifications;

/// <summary>
/// The test case runner used for the shared specification lifecycle. Creates a
/// <see cref="SpecificationTestRunner"/> so the fact executes against the shared
/// <see cref="Specification"/> instance.
/// </summary>
/// <param name="sharedInstance">The shared <see cref="Specification"/> instance every fact runs against.</param>
/// <param name="testCase">The test case to be run.</param>
/// <param name="displayName">The display name of the test case.</param>
/// <param name="skipReason">The skip reason, if the test is to be skipped.</param>
/// <param name="constructorArguments">The arguments to be passed to the test class constructor.</param>
/// <param name="testMethodArguments">The arguments to be passed to the test method.</param>
/// <param name="messageBus">The message bus to report run status to.</param>
/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
public class SpecificationTestCaseRunner(
    Specification sharedInstance,
    IXunitTestCase testCase,
    string displayName,
    string skipReason,
    object[] constructorArguments,
    object[] testMethodArguments,
    IMessageBus messageBus,
    ExceptionAggregator aggregator,
    CancellationTokenSource cancellationTokenSource) : XunitTestCaseRunner(testCase, displayName, skipReason, constructorArguments, testMethodArguments, messageBus, aggregator, cancellationTokenSource)
{
    /// <inheritdoc/>
    protected override XunitTestRunner CreateTestRunner(
        ITest test,
        IMessageBus messageBus,
        Type testClass,
        object[] constructorArguments,
        MethodInfo testMethod,
        object[] testMethodArguments,
        string skipReason,
        IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource)
        => new SpecificationTestRunner(
            sharedInstance,
            test,
            messageBus,
            testClass,
            constructorArguments,
            testMethod,
            testMethodArguments,
            skipReason,
            beforeAfterAttributes,
            new ExceptionAggregator(aggregator),
            cancellationTokenSource);
}
