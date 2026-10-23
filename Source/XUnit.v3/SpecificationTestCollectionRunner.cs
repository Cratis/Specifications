// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit.v3;

namespace Cratis.Specifications;

/// <summary>
/// Represents the test collection runner used by <see cref="SpecificationTestFramework"/>; it runs each
/// test class with its own <see cref="SpecificationTestClassRunner"/>.
/// </summary>
public class SpecificationTestCollectionRunner : XunitTestCollectionRunner
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SpecificationTestCollectionRunner"/> class.
    /// </summary>
    protected SpecificationTestCollectionRunner()
    {
    }

    /// <summary>
    /// Gets the singleton instance of the <see cref="SpecificationTestCollectionRunner"/>.
    /// </summary>
    public static new SpecificationTestCollectionRunner Instance { get; } = new();

    /// <inheritdoc/>
    protected override ValueTask<RunSummary> RunTestClass(
        XunitTestCollectionRunnerContext ctxt,
        IXunitTestClass? testClass,
        IReadOnlyCollection<IXunitTestCase> testCases)
        => testClass is null
            ? base.RunTestClass(ctxt, testClass, testCases)
            : new SpecificationTestClassRunner().Run(
                testClass,
                testCases,
                ctxt.ExplicitOption,
                ctxt.MessageBus,
                ctxt.Aggregator.Clone(),
                ctxt.CancellationTokenSource,
                ctxt.ParallelMode,
                ctxt.Scheduler,
                ctxt.CollectionFixtureMappings);
}
