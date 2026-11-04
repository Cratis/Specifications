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
    protected override bool ReportDiscoveredTestCase(ITestCase testCase, bool includeSourceInformation, IMessageBus messageBus)
    {
        if (IsStandardCase(testCase) && testCase is XunitTestCase xunitCase)
        {
            var type = testCase.TestMethod.TestClass.Class.ToRuntimeType();
            if (typeof(Specification).IsAssignableFrom(type) && type.Assembly.IsDefined(typeof(ReadableSpecificationNamesAttribute), false))
            {
                testCase = xunitCase is XunitTheoryTestCase theory
                    ? new SpecificationTheoryTestCase(DiagnosticMessageSink, theory)
                    : new SpecificationTestCase(DiagnosticMessageSink, xunitCase);
            }
        }
        return base.ReportDiscoveredTestCase(testCase, includeSourceInformation, messageBus);
    }
    static bool IsStandardCase(ITestCase testCase) =>
        testCase.GetType() == typeof(XunitTestCase) || testCase.GetType() == typeof(XunitTheoryTestCase);
}
