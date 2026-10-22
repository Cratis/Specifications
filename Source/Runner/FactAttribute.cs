// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications;

/// <summary>
/// Marks a method as a fact of a specification.
/// </summary>
/// <remarks>
/// Methods named should_* are facts by convention and do not need the attribute. It exists for parity with
/// the xUnit flavor, to ease migration, and to mark a fact as skipped.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class FactAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the reason for skipping the fact. The fact is skipped when this is set.
    /// </summary>
    public string? Skip { get; set; }
}
