// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;
using Cratis.Specifications.Runner.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Cratis.Specifications.Benchmarks;

[MemoryDiagnoser]
public class CompilationBenchmarks
{
    CSharpCompilation _compilation = null!;
    ImmutableArray<DiagnosticAnalyzer> _analyzers;

    const int Count = 500;

    [GlobalSetup]
    public void Setup()
    {
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Where(path => !Path.GetFileName(path).StartsWith("Cratis.Specifications.", StringComparison.Ordinal) || Path.GetFileName(path) == "Cratis.Specifications.Runner.dll")
            .Distinct().Select(path => MetadataReference.CreateFromFile(path));
        var trees = Enumerable.Range(0, Count).Select(index => CSharpSyntaxTree.ParseText($$"""
            namespace Workload;
            public class when_running_{{index}} : Cratis.Specifications.Specification
            {
                int _value;
                void Establish() => _value = 1;
                void Because() => _value++;
                public void should_pass() { if (_value != 2) throw new System.Exception(); }
            }
            """, path: $"when_running_{index}.cs"));
        _compilation = CSharpCompilation.Create("BenchmarkWorkload", trees, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = _compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0) throw new InvalidOperationException(string.Join("\n", errors.Select(error => error.ToString())));
        _analyzers = [.. typeof(Cratis.Specifications.CodeAnalysis.Analyzers.FactMustNotModifySharedContextAnalyzer).Assembly.GetTypes()
            .Where(type => !type.IsAbstract && typeof(DiagnosticAnalyzer).IsAssignableFrom(type))
            .Select(type => (DiagnosticAnalyzer)Activator.CreateInstance(type)!)];
        if (_analyzers.Length != 9) throw new InvalidOperationException($"Expected 9 analyzers, found {_analyzers.Length}.");
        if (!GenerateCold().GeneratedTrees.Any(tree => tree.GetText().ToString().Contains("when_running_499", StringComparison.Ordinal)))
        {
            throw new InvalidOperationException("Generator did not include all 500 specification classes.");
        }
    }

    [Benchmark]
    public GeneratorDriverRunResult GenerateCold()
    {
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new SpecificationGenerator());
        var result = driver.RunGenerators(_compilation).GetRunResult();
        if (result.GeneratedTrees.Length == 0 || result.Diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)) throw new InvalidOperationException("Generator did not produce a valid registry.");
        return result;
    }

    [Benchmark]
    public async Task<ImmutableArray<Diagnostic>> AnalyzeAll() => await Analyze().GetAnalyzerDiagnosticsAsync();

    public async Task WriteTelemetry(string path)
    {
        var run = Analyze();
        await run.GetAnalyzerDiagnosticsAsync();
        var rows = new List<string> { "Analyzer,ExecutionMilliseconds,SyntaxNodeActions,SymbolActions" };
        foreach (var analyzer in _analyzers)
        {
            var telemetry = await run.GetAnalyzerTelemetryInfoAsync(analyzer, CancellationToken.None);
            rows.Add($"{analyzer.GetType().Name},{telemetry.ExecutionTime.TotalMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture)},{telemetry.SyntaxNodeActionsCount},{telemetry.SymbolActionsCount}");
        }
        await File.WriteAllLinesAsync(path, rows);
    }

    CompilationWithAnalyzers Analyze() => _compilation.WithAnalyzers(_analyzers,
        new CompilationWithAnalyzersOptions(new AnalyzerOptions([]), null, concurrentAnalysis: false, logAnalyzerExecutionTime: true, reportSuppressedDiagnostics: false));
}[Benchmark(Baseline = true)]ht (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;
using Cratis.Specifications.Runner.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Cratis.Specifications.Benchmarks;

[MemoryDiagnoser]
public class CompilationBenchmarks
{
    CSharpCompilation _compilation = null!;
    ImmutableArray<DiagnosticAnalyzer> _analyzers;

    const int Count = 500;

    [GlobalSetup]
    public void Setup()
    {
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Where(path => !Path.GetFileName(path).StartsWith("Cratis.Specifications.", StringComparison.Ordinal) || Path.GetFileName(path) == "Cratis.Specifications.Runner.dll")
            .Distinct().Select(path => MetadataReference.CreateFromFile(path));
        var trees = Enumerable.Range(0, Count).Select(index => CSharpSyntaxTree.ParseText($$"""
            namespace Workload;
            public class when_running_{{index}} : Cratis.Specifications.Specification
            {
                int _value;
                void Establish() => _value = 1;
                void Because() => _value++;
                public void should_pass() { if (_value != 2) throw new System.Exception(); }
            }
            """, path: $"when_running_{index}.cs"));
        _compilation = CSharpCompilation.Create("BenchmarkWorkload", trees, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = _compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0) throw new InvalidOperationException(string.Join("\n", errors.Select(error => error.ToString())));
        _analyzers = [.. typeof(Cratis.Specifications.CodeAnalysis.Analyzers.FactMustNotModifySharedContextAnalyzer).Assembly.GetTypes()
            .Where(type => !type.IsAbstract && typeof(DiagnosticAnalyzer).IsAssignableFrom(type))
            .Select(type => (DiagnosticAnalyzer)Activator.CreateInstance(type)!)];
        if (_analyzers.Length != 9) throw new InvalidOperationException($"Expected 9 analyzers, found {_analyzers.Length}.");
        if (!GenerateCold().GeneratedTrees.Any(tree => tree.GetText().ToString().Contains("when_running_499", StringComparison.Ordinal)))
        {
            throw new InvalidOperationException("Generator did not include all 500 specification classes.");
        }
    }

    [Benchmark]
    public GeneratorDriverRunResult GenerateCold()
    {
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new SpecificationGenerator());
        var result = driver.RunGenerators(_compilation).GetRunResult();
        if (result.GeneratedTrees.Length == 0 || result.Diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)) throw new InvalidOperationException("Generator did not produce a valid registry.");
        return result;
    }

    [Benchmark]
    public async Task<ImmutableArray<Diagnostic>> AnalyzeAll() => await Analyze().GetAnalyzerDiagnosticsAsync();

    public async Task WriteTelemetry(string path)
    {
        var run = Analyze();
        await run.GetAnalyzerDiagnosticsAsync();
        var rows = new List<string> { "Analyzer,ExecutionMilliseconds,SyntaxNodeActions,SymbolActions" };
        foreach (var analyzer in _analyzers)
        {
            var telemetry = await run.GetAnalyzerTelemetryInfoAsync(analyzer, CancellationToken.None);
            rows.Add($"{analyzer.GetType().Name},{telemetry.ExecutionTime.TotalMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture)},{telemetry.SyntaxNodeActionsCount},{telemetry.SymbolActionsCount}");
        }
        await File.WriteAllLinesAsync(path, rows);
    }

    CompilationWithAnalyzers Analyze() => _compilation.WithAnalyzers(_analyzers,
        new CompilationWithAnalyzersOptions(new AnalyzerOptions([]), null, concurrentAnalysis: false, logAnalyzerExecutionTime: true, reportSuppressedDiagnostics: false));
}
