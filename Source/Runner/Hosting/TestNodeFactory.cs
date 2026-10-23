// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Testing.Extensions.TrxReport.Abstractions;
using Microsoft.Testing.Platform.Extensions.Messages;

namespace Cratis.Specifications.Runner;

/// <summary>
/// Creates the test nodes reported to Microsoft.Testing.Platform, reusing the immutable properties of every fact.
/// </summary>
/// <param name="assemblyFullName">The full name of the test assembly.</param>
/// <param name="trx">The TRX capability, telling whether TRX specific properties should be added.</param>
internal sealed class TestNodeFactory(string assemblyFullName, TrxCapability trx)
{
    /// <summary>
    /// Creates a node for a fact in a given state.
    /// </summary>
    /// <param name="specification">The specification class.</param>
    /// <param name="fact">The fact.</param>
    /// <param name="state">The state of the fact.</param>
    /// <returns>The node.</returns>
    public TestNode Create(SpecificationClassDescriptor specification, FactDescriptor fact, TestNodeStateProperty state) =>
        new()
        {
            Uid = new TestNodeUid(fact.Uid),
            DisplayName = fact.DisplayName,
            Properties = new PropertyBag([state, .. SharedProperties(specification, fact)])
        };

    /// <summary>
    /// Creates a node for the result of a fact.
    /// </summary>
    /// <param name="specification">The specification class.</param>
    /// <param name="fact">The fact.</param>
    /// <param name="result">The result.</param>
    /// <returns>The node.</returns>
    public TestNode Create(SpecificationClassDescriptor specification, FactDescriptor fact, FactResult result)
    {
        var timing = new TimingProperty(new TimingInfo(result.Started, result.Started + result.Duration, result.Duration));
        var shared = SharedProperties(specification, fact);
        var properties = result.Outcome switch
        {
            FactOutcome.Passed => [PassedTestNodeStateProperty.CachedInstance, timing, .. shared],
            FactOutcome.Skipped => [new SkippedTestNodeStateProperty(result.SkipReason), timing, .. shared],
            _ => Failed(result.Exception!, timing, shared, specification.Uid)
        };

        return new()
        {
            Uid = new TestNodeUid(fact.Uid),
            DisplayName = fact.DisplayName,
            Properties = new PropertyBag(properties)
        };
    }

    IProperty[] Failed(Exception exception, TimingProperty timing, IProperty[] shared, string className)
    {
        var state = new FailedTestNodeStateProperty(exception, exception.Message);
        return trx.IsEnabled
            ? [state, timing, new TrxExceptionProperty(exception.Message, exception.StackTrace), new TrxFullyQualifiedTypeNameProperty(className), .. shared]
            : [state, timing, .. shared];
    }

    IProperty[] SharedProperties(SpecificationClassDescriptor specification, FactDescriptor fact)
    {
        var properties = fact.Properties;
        if (properties.Length > 0) return properties;

        var method = new TestMethodIdentifierProperty(
            assemblyFullName,
            specification.Namespace,
            specification.TypeName,
            fact.MethodName,
            0,
            [],
            "System.Void");

        properties = fact.FilePath is null
            ? [method]
            : [method, new TestFileLocationProperty(fact.FilePath, new LinePositionSpan(new LinePosition(fact.Line, -1), new LinePosition(fact.Line, -1)))];
        fact.Properties = properties;
        return properties;
    }
}
