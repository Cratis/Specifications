// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit.Abstractions;
using Xunit.Sdk;

namespace Cratis.Specifications;

/// <summary>
/// Retains xUnit's deferred theory execution with readable names.
/// </summary>
public class SpecificationTheoryTestCase : XunitTheoryTestCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SpecificationTheoryTestCase"/> class for deserialization.
    /// </summary>
    [Obsolete("Called by the deserializer only.")]
    public SpecificationTheoryTestCase() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpecificationTheoryTestCase"/> class.
    /// </summary>
    /// <param name="diagnosticMessageSink">The diagnostic sink.</param>
    /// <param name="original">The standard deferred theory.</param>
    public SpecificationTheoryTestCase(IMessageSink diagnosticMessageSink, XunitTheoryTestCase original)
        : base(diagnosticMessageSink, original.DefaultMethodDisplay, original.DefaultMethodDisplayOptions, original.TestMethod)
    {
        SourceInformation = original.SourceInformation;
    }

    /// <inheritdoc/>
    protected override string GetDisplayName(IAttributeInfo factAttribute, string displayName) =>
        base.GetDisplayName(factAttribute, SpecificationTestCase.ReadableName(TestMethod));
}
