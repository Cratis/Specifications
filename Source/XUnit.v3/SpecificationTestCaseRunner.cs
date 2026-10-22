// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit.v3;

namespace Cratis.Specifications;

/// <summary>
/// Represents a test case runner that runs every test of a test case with a
/// <see cref="SpecificationTestRunner"/> bound to the shared specification instance.
/// </summary>
/// <param name="sharedInstance">The specification instance shared by every fact in the class.</param>
public class SpecificationTestCaseRunner(Specification sharedInstance) : XunitTestCaseRunner
{
    /// <inheritdoc/>
    protected override ValueTask<RunSummary> RunTest(XunitTestCaseRunnerContext ctxt, IXunitTest test)
        => new SpecificationTestRunner(sharedInstance).Run(
            test,
            ctxt.MessageBus,
            ctxt.ConstructorArguments,
            ctxt.ExplicitOption,
            ctxt.Aggregator.Clone(),
            ctxt.CancellationTokenSource,
            ctxt.ParallelMode,
            ctxt.Scheduler,
            ctxt.BeforeAfterTestAttributes,
            ctxt.CaseFixtureMappings);
}
