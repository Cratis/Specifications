// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications;

/// <summary>
/// Opts TUnit specifications into readable hierarchy names while leaving plain TUnit tests unchanged.
/// </summary>
public sealed class SpecificationDisplayNameAttribute : DisplayNameFormatterAttribute
{
    /// <inheritdoc/>
    protected override string FormatDisplayName(DiscoveredTestContext context)
    {
        var type = context.TestDetails.Class.ClassType;
        if (!typeof(Specification).IsAssignableFrom(type) || !type.Assembly.IsDefined(typeof(ReadableSpecificationNamesAttribute), false)) return context.GetDisplayName();
        var name = SpecificationDisplayNames.ForFact(SpecificationDisplayNames.ForType(type), context.TestName);
        var frameworkName = context.GetDisplayName();
        var arguments = frameworkName.IndexOf('(');
        return arguments < 0 ? name : string.Concat(name.AsSpan(), frameworkName.AsSpan(arguments));
    }
}
