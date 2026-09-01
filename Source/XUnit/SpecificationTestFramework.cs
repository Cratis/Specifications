// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Cratis.Specifications;

/// <summary>
/// An opt-in xUnit test framework that gives <see cref="Specification"/> classes MSpec-style semantics;
/// the test class is constructed once, Establish and Because run once, every fact runs against that
/// shared instance and Destroy runs once after the last fact.
/// </summary>
/// <param name="messageSink">The message sink used to send diagnostic messages.</param>
/// <remarks>
/// Opt in by adding the standard xUnit assembly attribute to the test assembly:
/// <code>
/// [assembly: Xunit.TestFramework("Cratis.Specifications.SpecificationTestFramework", "Cratis.Specifications.XUnit")]
/// </code>
/// The shared lifecycle applies only to classes deriving from <see cref="Specification"/> that have a
/// single public parameterless constructor, do not implement <see cref="IDisposable"/> and contain only
/// plain facts. Everything else - plain xUnit classes, theories, classes using fixtures or
/// <c>ITestOutputHelper</c> - runs with the stock xUnit per-fact lifecycle.
/// Facts must not mutate the shared context; they are assertions about the single execution the class
/// describes.
/// </remarks>
public class SpecificationTestFramework(IMessageSink messageSink) : XunitTestFramework(messageSink)
{
    /// <inheritdoc/>
    protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
        => new SpecificationTestFrameworkExecutor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);
}
