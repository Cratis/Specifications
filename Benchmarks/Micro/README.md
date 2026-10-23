<!-- Copyright (c) Cratis. All rights reserved. -->
<!-- Licensed under the MIT license. See LICENSE file in the project root for full license information. -->

# Microbenchmarks

BenchmarkDotNet coverage of the current implementation, not proposed optimizations.
All projects target .NET 10 and use `MemoryDiagnoser`. Workload construction and
reflection-to-delegate binding happen outside measured methods.

## Build and smoke checks

From the repository root:

```sh
dotnet build Benchmarks/Micro/Cratis.Specifications.Runner.Specs.csproj -c Release
dotnet build Benchmarks/Micro/XUnit2/XUnit2.csproj -c Release
dotnet build Benchmarks/Micro/XUnit3/XUnit3.csproj -c Release

dotnet run --project Benchmarks/Micro/Cratis.Specifications.Runner.Specs.csproj -c Release --no-build -- --job dry --filter '*' --artifacts .ai-work/perf/dry-core
dotnet run --project Benchmarks/Micro/XUnit2/XUnit2.csproj -c Release --no-build -- --job dry --filter '*' --artifacts .ai-work/perf/dry-xunit2
dotnet run --project Benchmarks/Micro/XUnit3/XUnit3.csproj -c Release --no-build -- --job dry --filter '*' --artifacts .ai-work/perf/dry-xunit3
```

Dry jobs check compilation and execution; they are **not performance results**.
Short jobs provide preliminary measurements with wide confidence intervals.
For recorded results omit `--job` (BenchmarkDotNet's default job), select a bounded
subset with `--filter`, and run through the repository's approved admission wrapper.
All failures, including failed benchmark processes and empty selections, return nonzero.

## Coverage and interpretation

| Group | Work measured |
|---|---|
| Lifecycle | Establish discovery/invocation, depth 1/3, sync/Task.Yield; cold cache versus warm cache |
| Assertions | Ordered equality, unordered containment, last-item containment; six flavors, 10/1,000 distinct integers |
| TUnit | Constructor Create/Release pair, fresh initialization, already-initialized semaphore path |
| Runner | Direct generated-style call versus cached reflection invocation; five sync/async facts, parallel/sequential; scheduler with 5,000 classes |
| Compilation | Fresh generator driver and all nine CRSPEC analyzers over 500 spec classes |
| xUnit 2/3 | Public shared-context class runner, five passing facts, including descriptor construction and message bus calls |

- The main executable deliberately uses the existing `Cratis.Specifications.Runner.Specs`
  friend assembly identity to access internal execution classes without changing product
  visibility. Its project filename must match that assembly name for BDN project discovery.
- xUnit versions use separate executables to avoid assembly/type collisions.
  Assertion flavors use reference aliases in the main executable.
- Cold lifecycle cache eviction is iteration setup, not measured work. BDN consequently
  uses one invocation per iteration for that method. Warm invocation does not clear caches.
  Async lifecycle/fact cases deliberately include real `Task.Yield` scheduling.
- Scheduler results are normalized **per class** (`OperationsPerInvoke = 5000`). A counting
  sink verifies every class and fact completed. Fact results are reused only after awaiting
  the preceding invocation.
- Direct calls can inline below timing resolution: do not derive a speedup ratio from a
  near-zero result. Neither this nor the reflection microcase is end-to-end runner throughput.
- Collections are distinct integer arrays with successful assertions. They do not establish
  duplicate, custom-comparer, exception-message or streaming-input semantics. Optimizations
  must preserve those contracts independently; runner/flavor implementations can differ.
- Compilation references and semantic binding are prepared outside timing. `GenerateCold`
  means a fresh generator driver, **not** a cold compiler process. Setup verifies all nine
  analyzers exist and the generated registry contains the 500th specification.
- Optional analyzer telemetry (single-threaded, synthetic workload, not consumer build cost):
  `dotnet run --project Benchmarks/Micro/Cratis.Specifications.Runner.Specs.csproj -c Release --no-build -- --analyzer-telemetry .ai-work/perf/analyzers.csv`.
  Use consumer builds with `ReportAnalyzer=true` and binlogs to assess shipped package cost.
- Descriptor creation and lightweight pass/fail guards are part of the class-runner timing.
  The sinks discard successful messages; a non-passing run fails the benchmark.

The local `Directory.Build.props` isolates BDN's generated single-framework host from the
repository's multi-targeting defaults and disables TUnit's unrelated generated entry point.
The editor configuration permits public BDN/xUnit entry types and convention-invoked private
lifecycle methods; other analyzers and warning-as-error builds remain enabled.
