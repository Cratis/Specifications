// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Cratis.Specifications;

/// <summary>
/// The test invoker used for the shared specification lifecycle. Returns the shared
/// <see cref="Specification"/> instance instead of constructing a new test class per fact.
/// </summary>
/// <param name="sharedInstance">The shared <see cref="Specification"/> instance every fact runs against.</param>
/// <param name="test">The test that this invocation belongs to.</param>
/// <param name="messageBus">The message bus to report run status to.</param>
/// <param name="testClass">The test class that the test method belongs to.</param>
/// <param name="constructorArguments">The arguments to be passed to the test class constructor.</param>
/// <param name="testMethod">The test method that will be invoked.</param>
/// <param name="testMethodArguments">The arguments to be passed to the test method.</param>
/// <param name="beforeAfterAttributes">The list of <see cref="BeforeAfterTestAttribute"/>s for this test invocation.</param>
/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
/// <remarks>
/// The stock <see cref="TestInvoker{TTestCase}"/> lifecycle still runs per fact against the shared
/// instance, harmlessly: <see cref="Specification.InitializeAsync"/> is idempotent,
/// <see cref="Specification.DisposeAsync"/> is a no-op for shared instances and eligible classes
/// never implement <see cref="IDisposable"/>, so per-fact disposal does not touch the instance.
/// </remarks>
public class SpecificationTestInvoker(
    Specification sharedInstance,
    ITest test,
    IMessageBus messageBus,
    Type testClass,
    object[] constructorArguments,
    MethodInfo testMethod,
    object[] testMethodArguments,
    IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes,
    ExceptionAggregator aggregator,
    CancellationTokenSource cancellationTokenSource) : XunitTestInvoker(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments, beforeAfterAttributes, aggregator, cancellationTokenSource)
{
    /// <inheritdoc/>
    /// <remarks>
    /// Mirrors the guard of the stock implementation - no instance is handed out for static test
    /// methods or when the aggregator already holds an exception (such as a failed Establish or
    /// Because), so the fact fails with that exception instead of executing.
    /// </remarks>
    protected override object? CreateTestClass()
        => !TestMethod.IsStatic && !Aggregator.HasExceptions ? sharedInstance : null;
}
