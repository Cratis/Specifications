// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit.v3;

namespace Cratis.Specifications;

/// <summary>
/// Retains xUnit's deferred theory execution and metadata with a readable display name.
/// </summary>
public class SpecificationTheoryTestCase : XunitDelayEnumeratedTheoryTestCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SpecificationTheoryTestCase"/> class for deserialization.
    /// </summary>
    [Obsolete("Called by the deserializer only.")]
    public SpecificationTheoryTestCase() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpecificationTheoryTestCase"/> class.
    /// </summary>
    /// <param name="original">The standard deferred theory.</param>
    public SpecificationTheoryTestCase(XunitDelayEnumeratedTheoryTestCase original)
        : base(original.TestMethod, SpecificationTestCase.Name(original), original.UniqueID, original.Explicit,
            original.SkipTestWithoutData, original.SkipExceptions, original.SkipReason, original.SkipType,
            original.SkipUnless, original.SkipWhen,
            original.Traits.ToDictionary(pair => pair.Key, pair => new HashSet<string>(pair.Value)),
            original.SourceFilePath, original.SourceLineNumber, original.Timeout)
    {
    }
}
