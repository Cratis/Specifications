// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Assertions;

/// <summary>
/// A class that does not derive from any package's Specification, used to prove a failing assertion still surfaces as the
/// package's own exception when the asserting code knows nothing about the test framework.
/// </summary>
public class PlainAsserter
{
    /// <summary>
    /// Runs an assertion that does not hold.
    /// </summary>
    public void AssertSomethingFalse() => 1.ShouldEqual(2);
}
