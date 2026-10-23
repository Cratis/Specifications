// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit.Sdk;
using Xunit.v3;

namespace Cratis.Specifications;

/// <summary>
/// Represents the test framework executor used by <see cref="SpecificationTestFramework"/>.
/// </summary>
/// <param name="testAssembly">The test assembly to run.</param>
public class SpecificationTestFrameworkExecutor(IXunitTestAssembly testAssembly) : XunitTestFrameworkExecutor(testAssembly)
{
    /// <inheritdoc/>
    public override async ValueTask RunTestCases(
        IReadOnlyCollection<IXunitTestCase> testCases,
        IMessageSink executionMessageSink,
        ITestFrameworkExecutionOptions executionOptions,
        CancellationToken cancellationToken)
        => await SpecificationTestAssemblyRunner.Instance.Run(TestAssembly, testCases, executionMessageSink, executionOptions, cancellationToken);
}
