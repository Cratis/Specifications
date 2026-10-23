// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Assertions;

/// <summary>
/// The outcome a catalogued assertion is expected to have.
/// </summary>
public enum ExpectedOutcome
{
    /// <summary>The assertion holds and nothing is thrown.</summary>
    Pass,

    /// <summary>The assertion fails with the framework's own assertion exception.</summary>
    Fail
}

/// <summary>
/// Represents one assertion example in the framework-free catalog shared by every package's specs.
/// </summary>
/// <param name="Name">Readable name of the case.</param>
/// <param name="Run">The assertion to run.</param>
/// <param name="Expected">The expected outcome.</param>
public record AssertionCase(string Name, Action Run, ExpectedOutcome Expected);
