// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using Xunit;
using Xunit.v3;

namespace Cratis.Specifications;

/// <summary>
/// A serializable standard xUnit v3 case with a readable specification name and unchanged identity.
/// </summary>
public class SpecificationTestCase : XunitTestCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SpecificationTestCase"/> class for deserialization.
    /// </summary>
    [Obsolete("Called by the deserializer only.")]
    public SpecificationTestCase()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpecificationTestCase"/> class.
    /// </summary>
    /// <param name="original">The standard case to name.</param>
    public SpecificationTestCase(XunitTestCase original)
        : base(original.TestMethod, Name(original), original.UniqueID, original.Explicit,
            original.TestLabel, original.DisableParallelization, original.SkipExceptions, original.SkipReason,
            original.SkipType, original.SkipUnless, original.SkipWhen,
            original.Traits.ToDictionary(pair => pair.Key, pair => new HashSet<string>(pair.Value)),
            original.TestMethodArguments, original.SourceFilePath, original.SourceLineNumber, original.Timeout)
    {
    }

    internal static string Name(IXunitTestCase original)
    {
        if (original.TestMethod.Method.GetCustomAttributes<FactAttribute>().Any(attribute => attribute.DisplayName is not null)) return original.TestCaseDisplayName;
        var suffix = original.TestCaseDisplayName.IndexOf('(');
        return SpecificationDisplayNames.ForFact(SpecificationDisplayNames.ForType(original.TestClass.Class), original.TestMethod.Method.Name)
            + (suffix < 0 ? string.Empty : original.TestCaseDisplayName.Substring(suffix));
    }
}
