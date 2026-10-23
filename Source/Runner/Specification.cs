// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications;

/// <summary>
/// Represents the base class for specifications run by the Cratis specification runner.
/// </summary>
/// <remarks>
/// The lifecycle is by convention and source generated; there is nothing to override:
/// - Establish (or Given) : establishes the context.
/// - Because (or When)     : performs the action.
/// - should_*              : the facts, the "Then" (a [Fact] attribute is accepted as well).
/// - Destroy               : cleans up the context.
/// .
/// Each lifecycle method may return void, Task or ValueTask and take no arguments. They are found up the
/// inheritance chain and run from the most basic type towards the specific type. Establish and Because run
/// once per specification class; every fact shares that context and may run concurrently with the others,
/// so facts should only read from it. Destroy runs once after the last fact, followed by disposal.
/// A class may not declare both Establish and Given (or both Because and When).
/// </remarks>
#pragma warning disable CA1052 // The type is a base class by design and carries no members.
public abstract class Specification;
#pragma warning restore CA1052
