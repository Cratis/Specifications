// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit.v3;

namespace Cratis.Specifications;

/// <summary>
/// Represents a test runner that runs a fact against the shared specification instance instead of
/// constructing a new instance of the class, and leaves disposal to the class runner.
/// </summary>
/// <param name="sharedInstance">The specification instance shared by every fact in the class.</param>
public class SpecificationTestRunner(Specification sharedInstance) : XunitTestRunner
{
    /// <inheritdoc/>
    protected override ValueTask<(object? Instance, SynchronizationContext? SyncContext, ExecutionContext? ExecutionContext)> CreateTestClassInstance(XunitTestRunnerContext ctxt)
        => new((sharedInstance, SynchronizationContext.Current, ExecutionContext.Capture()));

    /// <inheritdoc/>
    protected override bool IsTestClassDisposable(XunitTestRunnerContext ctxt, object testClassInstance) => false;
}
