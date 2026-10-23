// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using Xunit.v3;

namespace Cratis.Specifications;

/// <summary>
/// Represents an xUnit v3 test framework that runs every eligible <see cref="Specification"/> class
/// against a single shared instance, so Establish and Because run once per class instead of once per fact.
/// </summary>
/// <remarks>
/// <para>
/// Opt in by adding the following attribute to the test assembly:
/// <c>[assembly: Xunit.TestFramework(typeof(Cratis.Specifications.SpecificationTestFramework))]</c>.
/// </para>
/// <para>
/// xUnit v3 hands each level of the run (assembly, collection, class, method, test case, test) to a
/// runner, and every runner picks the next one. The framework therefore swaps in a thin subclass of the
/// stock runner at each level down to the test runner, which is the one that constructs the test class.
/// The class runner owns the shared instance; the lower runners only carry it to the test runner, which
/// hands it out instead of constructing a new instance per fact. Everything else is stock xUnit behavior.
/// </para>
/// <para>
/// A class is eligible when it derives from <see cref="Specification"/>, has a single public
/// constructor accepting only fixtures (or no parameters) and contains only plain
/// facts. Every other class keeps the stock xUnit per-fact lifecycle.
/// </para>
/// </remarks>
public class SpecificationTestFramework : XunitTestFramework
{
    readonly string? _configFileName;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpecificationTestFramework"/> class.
    /// </summary>
    public SpecificationTestFramework()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpecificationTestFramework"/> class.
    /// </summary>
    /// <param name="configFileName">The optional xUnit configuration file name.</param>
    public SpecificationTestFramework(string? configFileName)
        : base(configFileName)
    {
        _configFileName = configFileName;
    }

    /// <inheritdoc/>
    protected override ITestFrameworkDiscoverer CreateDiscoverer(Assembly assembly) =>
        new SpecificationTestFrameworkDiscoverer(new XunitTestAssembly(assembly, configFilePath: _configFileName));

    /// <inheritdoc/>
    protected override ITestFrameworkExecutor CreateExecutor(Assembly assembly)
        => new SpecificationTestFrameworkExecutor(new XunitTestAssembly(assembly, configFilePath: _configFileName));
}
