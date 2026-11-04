// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using TUnit.Core;

namespace Cratis.Specifications;

/// <summary>
/// Opts TUnit specifications into readable hierarchy names while leaving plain TUnit tests unchanged.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
public sealed class SpecificationDisplayNameAttribute : DisplayNameFormatterAttribute
{
    /// <inheritdoc/>
    protected override string FormatDisplayName(DiscoveredTestContext context)
    {
        var type = context.TestDetails.Class.ClassType;
        if (!typeof(Specification).IsAssignableFrom(type) || !type.Assembly.IsDefined(typeof(ReadableSpecificationNamesAttribute))) return context.GetDisplayName();
        return SpecificationDisplayNames.ForFact(SpecificationDisplayNames.ForType(type), context.TestName);
    }
}
