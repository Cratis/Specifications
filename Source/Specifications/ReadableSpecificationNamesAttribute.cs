// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications;

/// <summary>
/// Opts the test assembly into readable specification display names without changing test identities.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class ReadableSpecificationNamesAttribute : Attribute;
