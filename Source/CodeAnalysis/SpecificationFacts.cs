// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Cratis.Specifications.CodeAnalysis;

/// <summary>
/// Shared helpers for reasoning about Cratis specification types and their test methods.
/// </summary>
public static class SpecificationFacts
{
    /// <summary>
    /// The full name of the base type all specifications derive from.
    /// </summary>
    public const string SpecificationBaseTypeName = "Cratis.Specifications.Specification";

    /// <summary>
    /// The namespace segment that marks a reusable specification context.
    /// </summary>
    public const string GivenNamespaceSegment = "given";

    /// <summary>
    /// The suffix that marks a shared specification helper type exempt from the one-per-file rule.
    /// </summary>
    public const string SpecHelpersSuffix = "SpecHelpers";

    /// <summary>
    /// The prefix a specification fact method reads as.
    /// </summary>
    public const string ShouldPrefix = "should_";

    /// <summary>
    /// The convention-discovered lifecycle method names.
    /// </summary>
    public static readonly ImmutableArray<string> LifecycleMethodNames = ImmutableArray.Create("Establish", "Because", "Destroy");

    static readonly HashSet<string> _testMethodAttributeNames = new(System.StringComparer.Ordinal)
    {
        // xUnit
        "FactAttribute",
        "TheoryAttribute",

        // NUnit
        "TestAttribute",
        "TestCaseAttribute"
    };

    /// <summary>
    /// Determines whether the given type derives from the specification base type.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>True if the type derives from <see cref="SpecificationBaseTypeName"/>.</returns>
    public static bool DerivesFromSpecification(INamedTypeSymbol? type)
    {
        for (var current = type?.BaseType; current is not null; current = current.BaseType)
        {
            if (current.ToDisplayString() == SpecificationBaseTypeName)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the method is a test method (carries a known xUnit or NUnit test attribute).
    /// </summary>
    /// <param name="method">The method to inspect.</param>
    /// <returns>True if the method carries a recognized test attribute.</returns>
    public static bool IsTestMethod(IMethodSymbol method) =>
        method.GetAttributes().Any(attribute => IsTestMethodAttribute(attribute.AttributeClass));

    /// <summary>
    /// Determines whether the attribute type is a recognized xUnit or NUnit test attribute.
    /// </summary>
    /// <param name="attributeClass">The attribute type to inspect.</param>
    /// <returns>True if it is a recognized test attribute.</returns>
    public static bool IsTestMethodAttribute(INamedTypeSymbol? attributeClass) =>
        attributeClass is not null && _testMethodAttributeNames.Contains(attributeClass.Name);

    /// <summary>
    /// Determines whether the type and every type that contains it are publicly accessible.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>True if the type is effectively public.</returns>
    public static bool IsEffectivelyPublic(INamedTypeSymbol type)
    {
        for (var current = type; current is not null; current = current.ContainingType)
        {
            if (current.DeclaredAccessibility != Accessibility.Public)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Determines whether the type declares at least one test method.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>True if any member is a test method.</returns>
    public static bool HasTestMethod(INamedTypeSymbol type) =>
        type.GetMembers().OfType<IMethodSymbol>().Any(IsTestMethod);

    /// <summary>
    /// Determines whether the type is declared inside a 'given' namespace.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>True if any containing namespace segment is 'given'.</returns>
    public static bool IsInGivenNamespace(INamedTypeSymbol type)
    {
        for (var ns = type.ContainingNamespace; ns is not null && !ns.IsGlobalNamespace; ns = ns.ContainingNamespace)
        {
            if (ns.Name == GivenNamespaceSegment)
            {
                return true;
            }
        }

        return false;
    }
}
