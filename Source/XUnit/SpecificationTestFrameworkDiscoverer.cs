// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit.Abstractions;
using Xunit.Sdk;

namespace Cratis.Specifications;

/// <summary>
/// Applies readable names during discovery without replacing the framework's test cases.
/// </summary>
/// <param name="assemblyInfo">The test assembly.</param>
/// <param name="sourceProvider">The source information provider.</param>
/// <param name="diagnosticMessageSink">The diagnostic sink.</param>
public class SpecificationTestFrameworkDiscoverer(IAssemblyInfo assemblyInfo, ISourceInformationProvider sourceProvider, IMessageSink diagnosticMessageSink)
    : XunitTestFrameworkDiscoverer(assemblyInfo, sourceProvider, diagnosticMessageSink)
{
    /// <inheritdoc/>
    protected override bool FindTestsForMethod(ITestMethod testMethod, bool includeSourceInformation, IMessageBus messageBus, ITestFrameworkDiscoveryOptions discoveryOptions)
    {
        using var namingBus = new NamingMessageBus(messageBus, DiagnosticMessageSink, discoveryOptions);
        return base.FindTestsForMethod(testMethod, includeSourceInformation, namingBus, discoveryOptions);
    }

    sealed class NamingMessageBus(IMessageBus inner, IMessageSink diagnosticMessageSink, ITestFrameworkDiscoveryOptions discoveryOptions) : IMessageBus
    {
        public bool QueueMessage(IMessageSinkMessage message) => inner.QueueMessage(NameMessage(message));

        IMessageSinkMessage NameMessage(IMessageSinkMessage message)
        {
            if (message is not ITestCaseDiscoveryMessage discovery || discovery.TestCase is not XunitTestCase testCase) return message;
            if (!IsStandardCase(testCase)) return message;

            var type = testCase.TestMethod.TestClass.Class.ToRuntimeType();
            if (!typeof(Specification).IsAssignableFrom(type) || !type.Assembly.IsDefined(typeof(ReadableSpecificationNamesAttribute), false)) return message;

            // Ownership of the named case transfers to xUnit through the discovery message; xUnit disposes test cases.
#pragma warning disable CA2000
            return new TestCaseDiscoveryMessage(CreateNamedCase(testCase));
#pragma warning restore CA2000
        }

        // The returned case is transferred to xUnit through the discovery message.
        XunitTestCase CreateNamedCase(XunitTestCase testCase) => testCase is XunitTheoryTestCase theory
            ? new SpecificationTheoryTestCase(diagnosticMessageSink, theory, discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault())
            : new SpecificationTestCase(diagnosticMessageSink, testCase, discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault());

        public void Dispose() { }

        static bool IsStandardCase(ITestCase testCase) =>
            testCase.GetType() == typeof(XunitTestCase) || testCase.GetType() == typeof(XunitTheoryTestCase);
    }
}
