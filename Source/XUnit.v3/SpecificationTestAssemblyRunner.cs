// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit.v3;

namespace Cratis.Specifications;

/// <summary>
/// Represents the test assembly runner used by <see cref="SpecificationTestFramework"/>; it runs each
/// test collection with a <see cref="SpecificationTestCollectionRunner"/>.
/// </summary>
public class SpecificationTestAssemblyRunner : XunitTestAssemblyRunner
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SpecificationTestAssemblyRunner"/> class.
    /// </summary>
    protected SpecificationTestAssemblyRunner()
    {
    }

    /// <summary>
    /// Gets the singleton instance of the <see cref="SpecificationTestAssemblyRunner"/>.
    /// </summary>
    public static new SpecificationTestAssemblyRunner Instance { get; } = new();

    /// <inheritdoc/>
    /// <remarks>
    /// Ends the specification session after the last test collection has run, before the assembly fixtures are
    /// disposed. A failure is reported as a test assembly cleanup failure.
    /// </remarks>
    protected override async ValueTask<bool> OnTestAssemblyFinished(XunitTestAssemblyRunnerContext ctxt, RunSummary summary)
    {
        await ctxt.Aggregator.RunAsync(SpecificationSession.End);
        return await base.OnTestAssemblyFinished(ctxt, summary);
    }

    /// <inheritdoc/>
    protected override ValueTask<RunSummary> RunTestCollection(
        XunitTestAssemblyRunnerContext ctxt,
        IXunitTestCollection testCollection,
        IReadOnlyCollection<IXunitTestCase> testCases)
        => SpecificationTestCollectionRunner.Instance.Run(
            testCollection,
            testCases,
            ctxt.ExplicitOption,
            ctxt.MessageBus,
            ctxt.Aggregator.Clone(),
            ctxt.CancellationTokenSource,
            ctxt.ParallelMode,
            ctxt.Scheduler,
            ctxt.AssemblyFixtureMappings);
}
