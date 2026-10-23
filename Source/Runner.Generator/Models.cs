// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.Generator;

/// <summary>
/// Defines what a lifecycle method or fact returns.
/// </summary>
public enum ReturnKind
{
    /// <summary>Returns nothing.</summary>
    Void = 0,

    /// <summary>Returns a Task.</summary>
    Task = 1,

    /// <summary>Returns a ValueTask.</summary>
    ValueTask = 2
}

/// <summary>
/// Defines how generated code calls a method.
/// </summary>
public enum CallKind
{
    /// <summary>A direct call; the method is accessible from the test assembly.</summary>
    Direct = 0,

    /// <summary>Through an UnsafeAccessor; no reflection, works for private and protected methods.</summary>
    Accessor = 1,

    /// <summary>Through reflection; only for inaccessible methods on constructed generic types.</summary>
    Reflection = 2
}

/// <summary>
/// Represents a call to a parameterless instance method.
/// </summary>
/// <param name="DeclaringType">The fully qualified declaring type.</param>
/// <param name="Name">The method name.</param>
/// <param name="Returns">What the method returns.</param>
/// <param name="How">How the method is called.</param>
public sealed record MethodCall(string DeclaringType, string Name, ReturnKind Returns, CallKind How);

/// <summary>
/// Represents a fact.
/// </summary>
/// <param name="Call">The call to the fact method.</param>
/// <param name="DisplayName">The readable Given/When/Then name.</param>
/// <param name="FilePath">The declaring source file.</param>
/// <param name="Line">The 1-based line of the declaration.</param>
/// <param name="SkipReason">The skip reason, if skipped.</param>
public sealed record FactModel(MethodCall Call, string DisplayName, string? FilePath, int Line, string? SkipReason);

/// <summary>
/// Represents a specification class.
/// </summary>
/// <param name="Uid">The stable unique identifier.</param>
/// <param name="Namespace">The namespace.</param>
/// <param name="TypeName">The type name, nested types separated by '+'.</param>
/// <param name="DisplayName">The readable Given/When/Then name.</param>
/// <param name="FullyQualifiedName">The fully qualified name to use in code.</param>
/// <param name="ConstructorIsAccessible">Whether the parameterless constructor can be called directly.</param>
/// <param name="Establish">The Establish (or Given) chain, most basic type first.</param>
/// <param name="Because">The Because (or When) chain, most basic type first.</param>
/// <param name="Destroy">The Destroy chain, most basic type first.</param>
/// <param name="Facts">The facts.</param>
/// <param name="Error">A discovery error, if any.</param>
/// <param name="Diagnostics">The diagnostics to report for the class.</param>
public sealed record SpecificationModel(
    string Uid,
    string Namespace,
    string TypeName,
    string DisplayName,
    string FullyQualifiedName,
    bool ConstructorIsAccessible,
    EquatableArray<MethodCall> Establish,
    EquatableArray<MethodCall> Because,
    EquatableArray<MethodCall> Destroy,
    EquatableArray<FactModel> Facts,
    string? Error,
    EquatableArray<DiagnosticModel> Diagnostics);
