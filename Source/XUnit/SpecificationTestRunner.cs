// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Cratis.Specifications;

/// <summary>
/// The test runner used for the shared specification lifecycle. Invokes the test method through
/// <see cref="SpecificationTestInvoker"/> so the fact executes against the shared
/// <see cref="Specification"/> instance.
/// </summary>
/// <param name="sharedInstance">The shared <see cref="Specification"/> instance every fact runs against.</param>
/// <param name="test">The test that this invocation belongs to.</param>
/// <param name="messageBus">The message bus to report run status to.</param>
/// <param name="testClass">The test class that the test method belongs to.</param>
/// <param name="constructorArguments">The arguments to be passed to the test class constructor.</param>
/// <param name="testMethod">The test method that will be invoked.</param>
/// <param name="testMethodArguments">The arguments to be passed to the test method.</param>
/// <param name="skipReason">The skip reason, if the test is to be skipped.</param>
/// <param name="beforeAfterAttributes">The list of <see cref="BeforeAfterTestAttribute"/>s for this test.</param>
/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
public class SpecificationTestRunner(
    Specification sharedInstance,
    ITest test,
    IMessageBus messageBus,
    Type testClass,
    object[] constructorArguments,
    MethodInfo testMethod,
    object[] testMethodArguments,
    string skipReason,
    IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes,
    ExceptionAggregator aggregator,
    CancellationTokenSource cancellationTokenSource) : XunitTestRunner(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments, skipReason, beforeAfterAttributes, aggregator, cancellationTokenSource)
{
    /// <inheritdoc/>
    protected override Task<decimal> InvokeTestMethodAsync(ExceptionAggregator aggregator)
        => new SpecificationTestInvoker(
            sharedInstance,
            Test,
            MessageBus,
            TestClass,
            ConstructorArguments,
            TestMethod,
            TestMethodArguments,
            BeforeAfterAttributes,
            aggregator,
            CancellationTokenSource).RunAsync();
}
