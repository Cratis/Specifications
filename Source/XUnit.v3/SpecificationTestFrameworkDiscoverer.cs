// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit.Sdk;
using Xunit.v3;

namespace Cratis.Specifications;

/// <summary>
/// Applies readable names during discovery without changing test identities or execution metadata.
/// </summary>
/// <param name="testAssembly">The test assembly.</param>
public class SpecificationTestFrameworkDiscoverer(IXunitTestAssembly testAssembly) : XunitTestFrameworkDiscoverer(testAssembly)
{
    /// <inheritdoc/>
    protected override ValueTask<bool> FindTestsForMethod(IXunitTestMethod testMethod, ITestFrameworkDiscoveryOptions discoveryOptions, Func<ITestCase, ValueTask<bool>> discoveryCallback) =>
        base.FindTestsForMethod(testMethod, discoveryOptions, testCase =>
        {
            if (IsStandardCase(testCase) && testCase is XunitTestCase xunitCase)
            {
                var type = xunitCase.TestClass.Class;
                if (typeof(Specification).IsAssignableFrom(type) && type.Assembly.IsDefined(typeof(ReadableSpecificationNamesAttribute), false))
                {
                    testCase = xunitCase is XunitDelayEnumeratedTheoryTestCase theory
                        ? new SpecificationTheoryTestCase(theory)
                        : new SpecificationTestCase(xunitCase);
                }
            }
            return discoveryCallback(testCase);
        });
    static bool IsStandardCase(ITestCase testCase) =>
        testCase.GetType() == typeof(XunitTestCase) || testCase.GetType() == typeof(XunitDelayEnumeratedTheoryTestCase);
}
