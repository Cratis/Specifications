// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Cratis.Specifications;

/// <summary>
/// The test framework executor for <see cref="SpecificationTestFramework"/>, running test cases
/// through <see cref="SpecificationTestAssemblyRunner"/>.
/// </summary>
/// <param name="assemblyName">Name of the test assembly.</param>
/// <param name="sourceInformationProvider">The source line number information provider.</param>
/// <param name="diagnosticMessageSink">The message sink to report diagnostic messages to.</param>
public class SpecificationTestFrameworkExecutor(
    AssemblyName assemblyName,
    ISourceInformationProvider sourceInformationProvider,
    IMessageSink diagnosticMessageSink) : XunitTestFrameworkExecutor(assemblyName, sourceInformationProvider, diagnosticMessageSink)
{
    /// <inheritdoc/>
    protected override async void RunTestCases(IEnumerable<IXunitTestCase> testCases, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions)
    {
        using var assemblyRunner = new SpecificationTestAssemblyRunner(TestAssembly, testCases, DiagnosticMessageSink, executionMessageSink, executionOptions);
        await assemblyRunner.RunAsync();
    }
}
