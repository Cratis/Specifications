// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications;

/// <summary>
/// Retains xUnit's per-fact lifecycle for the marked assembly or specification class.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class)]
public sealed class PerFactSpecificationLifecycleAttribute : Attribute;
