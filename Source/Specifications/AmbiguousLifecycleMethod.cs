// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications;

/// <summary>
/// A declaring type uses both names of a single lifecycle phase.
/// </summary>
/// <param name="type">The type with the ambiguous phase.</param>
/// <param name="primary">The conventional lifecycle name.</param>
/// <param name="alias">The alternate lifecycle name.</param>
public class AmbiguousLifecycleMethod(Type type, string primary, string alias)
    : Exception($"{type.Name} declares both {primary} and {alias}; use one of them.");
