// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Cratis.Specifications.Runner.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cratis.Specifications.Runner.given;

/// <summary>
/// Runs the specification generator over a piece of source code.
/// </summary>
internal static class a_generator_run
{
    static readonly MetadataReference[] _references = [.. ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))
        .Split(Path.PathSeparator)
        .Select(_ => MetadataReference.CreateFromFile(_))];

    public static (ImmutableArray<Diagnostic> Diagnostics, string Source) Over(string source)
    {
        var compilation = CSharpCompilation.Create(
            "GeneratorProbe",
            [CSharpSyntaxTree.ParseText(source)],
            _references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Disable));
        var driver = CSharpGeneratorDriver.Create(new SpecificationGenerator()).RunGenerators(compilation);
        var result = driver.GetRunResult();
        return (result.Diagnostics, string.Concat(result.GeneratedTrees.Select(_ => _.ToString())));
    }
}
