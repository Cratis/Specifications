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
        var timing = Timing(result);
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

    /// <summary>
    /// Creates a separate failed node for cleanup of a selected specification class.
    /// </summary>
    /// <param name="specification">The class whose teardown failed.</param>
    /// <param name="result">The aggregated teardown failure.</param>
    /// <returns>The failed teardown node.</returns>
    public TestNode CreateTearDown(SpecificationClassDescriptor specification, FactResult result)
    {
        var method = new TestMethodIdentifierProperty(assemblyFullName, specification.Namespace, specification.TypeName, "(teardown)", 0, [], "System.Void");
        return new()
        {
            Uid = new TestNodeUid(specification.Uid + ".(teardown)"),
            DisplayName = specification.DisplayName + " (teardown)",
            Properties = new PropertyBag(Failed(result.Exception!, Timing(result), [method], specification.Uid))
        };
    }

    static TimingProperty Timing(FactResult result)
    {
        var info = new TimingInfo(result.Started, result.Started + result.Duration, result.Duration);
        return result.Steps is null ? new TimingProperty(info) : new TimingProperty(info, Steps(result.Steps));
    }

    static StepTimingInfo[] Steps(IReadOnlyList<PhaseRecord> phases)
    {
        var steps = new StepTimingInfo[phases.Count];
        for (var index = 0; index < phases.Count; index++)
        {
            var phase = phases[index];
            steps[index] = new StepTimingInfo(phase.Phase.ToString(), phase.Phase.ToString(), new TimingInfo(phase.StartedUtc, phase.StartedUtc + phase.Duration, phase.Duration));
        }

        return steps;
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
            fact.ReturnTypeFullName);

        properties = fact.FilePath is null
            ? [method]
            : [method, new TestFileLocationProperty(fact.FilePath, new LinePositionSpan(new LinePosition(fact.Line, -1), new LinePosition(fact.Line, -1)))];
        fact.Properties = properties;
        return properties;
    }
}
