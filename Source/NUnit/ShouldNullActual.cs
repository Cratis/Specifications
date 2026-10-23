// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

namespace Cratis.Specifications;

/// <summary>
/// Fails an assertion, with a readable message, when the actual value is null.
/// </summary>
static class NullActual
{
    [DoesNotReturn]
    public static void Fail(string assertion, string expectation) =>
        Assert.Fail($"{assertion} expected {expectation} but the actual value was null");
}
