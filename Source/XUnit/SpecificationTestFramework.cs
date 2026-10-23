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
/// single public constructor accepting only class or collection fixtures and contain only
/// plain facts. Everything else - plain xUnit classes, theories, custom async lifetimes or
/// <c>ITestOutputHelper</c> - runs with the stock xUnit per-fact lifecycle.
/// Disposal is owned by the class runner after Destroy; async disposal takes precedence over sync disposal.
/// Facts must not mutate the shared context; they are assertions about the single execution the class
/// describes.
/// </remarks>
public class SpecificationTestFramework(IMessageSink messageSink) : XunitTestFramework(messageSink)
{
    /// <inheritdoc/>
    protected override ITestFrameworkDiscoverer CreateDiscoverer(IAssemblyInfo assemblyInfo) =>
        new SpecificationTestFrameworkDiscoverer(assemblyInfo, SourceInformationProvider, DiagnosticMessageSink);

    /// <inheritdoc/>
    protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
    {
        PhaseTimingOutput.UseDiagnosticSink(DiagnosticMessageSink);
        return new SpecificationTestFrameworkExecutor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);
    }
}
