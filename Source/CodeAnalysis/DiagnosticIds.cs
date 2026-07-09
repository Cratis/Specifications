// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.CodeAnalysis;

/// <summary>
/// Diagnostic IDs for the Cratis Specifications analyzers.
/// </summary>
public static class DiagnosticIds
{
    /// <summary>
    /// A test method (e.g. [Fact]/[Test]) inside a Specification-derived class must be named 'should_*'.
    /// </summary>
    public const string FactMethodMustBeNamedShould = "CRSPEC0001";

    /// <summary>
    /// A file must declare at most one Specification-derived class.
    /// </summary>
    public const string OneSpecificationPerFile = "CRSPEC0002";

    /// <summary>
    /// A test method must not be declared on a Specification context type in a 'given' namespace.
    /// </summary>
    public const string FactOnGivenBaseClass = "CRSPEC0003";

    /// <summary>
    /// A 'should_*' method inside a Specification must carry a test attribute, otherwise it never runs.
    /// </summary>
    public const string ShouldMethodMustBeTestMethod = "CRSPEC0004";

    /// <summary>
    /// A lifecycle method (Establish/Because/Destroy) must not call its base implementation explicitly.
    /// </summary>
    public const string DoNotCallBaseLifecycleMethod = "CRSPEC0005";

    /// <summary>
    /// A Specification that declares test methods must be public so the runner discovers it.
    /// </summary>
    public const string SpecificationMustBePublic = "CRSPEC0006";

    /// <summary>
    /// The action under test (Because) must not be declared on a reusable 'given' context.
    /// </summary>
    public const string BecauseMustNotBeOnGivenContext = "CRSPEC0007";
}
