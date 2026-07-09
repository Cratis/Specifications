// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Cratis.Specifications.CodeAnalysis.Specs.Testing;

/// <summary>
/// Builds compilable C# source for analyzer tests, including stub declarations for the
/// <c>Cratis.Specifications.Specification</c> base and the xUnit/NUnit test attributes.
/// </summary>
public static class SpecSource
{
    /// <summary>
    /// Wrap the supplied usage in a namespace together with the stub framework declarations.
    /// </summary>
    /// <param name="usage">The C# declarations under test.</param>
    /// <param name="namespaceName">The namespace to place the usage in.</param>
    /// <returns>The full compilable source.</returns>
    public static string Wrap(string usage, string namespaceName = "Sample")
    {
        return string.Join(Environment.NewLine, new[]
        {
            "using System;",
            "",
            "namespace Cratis.Specifications",
            "{",
            "    public class Specification { }",
            "}",
            "",
            "namespace Xunit",
            "{",
            "    [AttributeUsage(AttributeTargets.Method)]",
            "    public sealed class FactAttribute : Attribute { public string Skip { get; set; } }",
            "    [AttributeUsage(AttributeTargets.Method)]",
            "    public sealed class TheoryAttribute : Attribute { }",
            "}",
            "",
            "namespace NUnit.Framework",
            "{",
            "    [AttributeUsage(AttributeTargets.Method)]",
            "    public sealed class TestAttribute : Attribute { }",
            "    [AttributeUsage(AttributeTargets.Method)]",
            "    public sealed class TestCaseAttribute : Attribute { }",
            "}",
            "",
            $"namespace {namespaceName}",
            "{",
            usage,
            "}"
        });
    }
}
