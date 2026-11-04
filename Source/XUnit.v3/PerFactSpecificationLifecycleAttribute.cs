// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications;

/// <summary>
/// Retains xUnit's per-fact lifecycle when the Cratis framework is registered only for readable names.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class PerFactSpecificationLifecycleAttribute : Attribute;
