// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner;

/// <summary>
/// Defines the outcome of a fact.
/// </summary>
internal enum FactOutcome
{
    /// <summary>The fact passed.</summary>
    Passed = 0,

    /// <summary>The fact, or the context it depends on, failed.</summary>
    Failed = 1,

    /// <summary>The fact was skipped.</summary>
    Skipped = 2
}
