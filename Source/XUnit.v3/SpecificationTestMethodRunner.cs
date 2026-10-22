// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit.v3;

namespace Cratis.Specifications;

/// <summary>
/// Represents a test method runner that runs every test case of a method with a
/// <see cref="SpecificationTestCaseRunner"/> bound to the shared specification instance.
/// </summary>
/// <param name="sharedInstance">The specification instance shared by every fact in the class.</param>
public class SpecificationTestMethodRunner(Specification sharedInstance) : XunitTestMethodRunner
{
    /// <inheritdoc/>
    protected override async ValueTask<RunSummary> RunTestCase(XunitTestMethodRunnerContext ctxt, IXunitTestCase testCase)
    {
        var aggregator = ctxt.Aggregator.Clone();
        var tests = await aggregator.RunAsync(testCase.CreateTests, []);
        var exception = aggregator.ToException();
        if (exception is not null)
        {
            return XunitRunnerHelper.FailTestCases(ctxt.MessageBus, ctxt.CancellationTokenSource, [testCase], exception);
        }

        return await new SpecificationTestCaseRunner(sharedInstance).Run(
            testCase,
            tests,
            ctxt.MessageBus,
            aggregator,
            ctxt.CancellationTokenSource,
            ctxt.ParallelMode,
            ctxt.Scheduler,
            testCase.TestCaseDisplayName,
            testCase.SkipReason,
            ctxt.ExplicitOption,
            ctxt.ConstructorArguments,
            ctxt.MethodFixtureMappings);
    }
}
