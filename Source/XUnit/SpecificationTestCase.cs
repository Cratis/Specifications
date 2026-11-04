// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit.Abstractions;
using Xunit.Sdk;

namespace Cratis.Specifications;

/// <summary>
/// A serializable standard xUnit case whose display name follows the specification hierarchy.
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
    /// <param name="diagnosticMessageSink">The diagnostic sink.</param>
    /// <param name="original">The standard case to name.</param>
    public SpecificationTestCase(IMessageSink diagnosticMessageSink, XunitTestCase original)
        : base(diagnosticMessageSink, original.DefaultMethodDisplay, original.DefaultMethodDisplayOptions, original.TestMethod, original.TestMethodArguments)
    {
        SourceInformation = original.SourceInformation;
    }

    /// <inheritdoc/>
    protected override string GetDisplayName(IAttributeInfo factAttribute, string displayName) =>
        base.GetDisplayName(factAttribute, ReadableName(TestMethod));

    internal static string ReadableName(ITestMethod method) =>
        SpecificationDisplayNames.ForFact(SpecificationDisplayNames.ForType(method.TestClass.Class.ToRuntimeType()), method.Method.Name);
}
