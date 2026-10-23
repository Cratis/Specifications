# Running specifications fast

How to keep a Cratis Specifications suite quick to run, locally and in CI. This is a how-to guide; the
[README](https://github.com/Cratis/Specifications/blob/main/README.md) describes each feature in full.

## Choose a package with speed in mind

Most of the time in a small suite is runner startup, discovery and reporting, not your specifications.
How you launch the suite matters more than the framework underneath.

These are indicative numbers, not a ranking: median CPU seconds for 1,500 trivial tests, measured on a loaded Mac
with the repository's [benchmark harness](https://github.com/Cratis/Specifications/blob/main/Benchmarks/README.md).
Measure your own suite before deciding.

| Package and launch path | Median CPU (s) |
| --- | --- |
| xUnit v2 through `dotnet test` | 1.46 |
| xUnit v3 as an executable | 0.75 |
| xUnit v3 through `dotnet test` | 3.40 |
| TUnit as an executable | 1.42 |
| Native runner as an executable | 0.86 |
| Native runner through `dotnet run` | 1.34 |

What to take from it:

- The same xUnit v3 suite costs about 4.5 times more CPU through `dotnet test` than as an executable in this run.
  Running the executable is the cheap win.
- The native runner (`Cratis.Specifications.Runner`, [experimental](https://github.com/Cratis/Specifications/blob/main/README.md#native-runner-on-microsoft-testing-platform-experimental))
  and xUnit v3 executables start fastest. Choose the native runner when you want its features (cache, sharding,
  readable names) and accept that it may change without notice.
- TUnit runs on Microsoft.Testing.Platform. See
  [Using TUnit](https://github.com/Cratis/Specifications/blob/main/README.md#using-tunit) for how it coexists with VSTest projects.
- Keep xUnit v2 or NUnit when you depend on their adapters and tooling; they are not slow for typical suites.

A tiny suite mostly measures startup. The gap between runners shrinks as your specifications do real work.

## Share the lifecycle

xUnit builds a new instance for every fact, so `Establish` and `Because` run once per fact. A shared lifecycle
runs them once per class, which is what a specification means: one context, one action, several assertions.

- NUnit, TUnit and the native runner already share the instance.
- On xUnit v2 and v3, opt in with `<CratisSpecificationsSharedLifecycle>true</CratisSpecificationsSharedLifecycle>`.
  Eligibility rules are in the README's
  [shared instance section](https://github.com/Cratis/Specifications/blob/main/README.md#shared-instance-per-specification-class-opt-in).
- The saving is `Establish` plus `Because` multiplied by the facts you no longer repeat. A class with 5 facts
  and a 200 ms `Because` goes from about 1 s of setup to about 200 ms. The benchmark harness's `--work-us` option
  adds deterministic work to each context so you can see this cost on your own hardware.
- Facts must only read the shared context. Raise `CRSPEC0008` to a warning with
  `<CratisSpecificationsWarnOnSharedContextMutation>true</CratisSpecificationsWarnOnSharedContextMutation>`
  so a fact that mutates it is caught at build time.

## Run the executable, not the full `dotnet test` pipeline

`dotnet test` adds MSBuild evaluation and the VSTest host on every run. For a local loop:

```bash
dotnet build Source/MyProject.Specs --configuration Release
dotnet run --project Source/MyProject.Specs --configuration Release --no-build
# or run the built apphost directly
```

TUnit and the native runner are executables already. xUnit v3 projects set `<OutputType>Exe</OutputType>`.
Where a repository mixes Microsoft.Testing.Platform and VSTest projects, run each project on its own, as this
repository's [build workflow](https://github.com/Cratis/Specifications/blob/main/.github/workflows/build.yml) does;
`dotnet test` on the whole solution does not work there.

## Filter, shard and cache

The native runner options (see the [README](https://github.com/Cratis/Specifications/blob/main/README.md#native-runner-on-microsoft-testing-platform-experimental)):

| Option | Use |
| --- | --- |
| `--treenode-filter '/*/*/when_registering/*'` | Run one class or fact by code name; wildcards and alternatives are supported. |
| `--filter-uid` | Run a class or fact by UID. |
| `--shard <index>/<count>` | Split the suite across CI jobs, for example `--shard 1/4`; balanced by cached durations when the cache is present. |
| `--specs-cache <path\|none>` | Path of the run cache (durations and failures); defaults to `.specs-cache` next to the test assembly, `none` disables it. In CI, restore it between runs so scheduling uses earlier durations. |
| `--last-failed-first` | Run previously failed specifications first. |
| `--fail-fast` | Stop on the first failure. |
| `--specs-parallel <n>` | Maximum number of classes running at once; `1` runs them one by one. |
| `--specs-sequential-facts` | Run the facts of a class one at a time. |

Pass these after `--`: `dotnet run --project <project> --no-build -- --shard 1/4`.

For xUnit and NUnit use the framework's own filters (`dotnet test --filter`) and settings, for example
`parallelizeTestCollections` and `maxParallelThreads` in `xunit.runner.json`, or `NUnit.NumberOfTestWorkers=0` passed
as a run parameter after `--`.

## Avoid repeated restores and builds

- Restore once, then pass `--no-restore` and `--no-build` to later `dotnet test` or `dotnet run` calls.
- Cache `~/.nuget/packages` in CI, keyed on `Directory.Packages.props` and the project files, as the
  [build workflow](https://github.com/Cratis/Specifications/blob/main/.github/workflows/build.yml) does.
- Build once in `Release` and run every spec project with `--no-build`.

## Skip the analyzers in large builds

The bundled analyzers add build time. Set `<CratisSpecificationsAnalyzers>false</CratisSpecificationsAnalyzers>` to turn
them off, or `ide` to keep them in the editor and drop them from command-line and CI builds. See
[Turning the analyzers off](https://github.com/Cratis/Specifications/blob/main/README.md#turning-the-analyzers-off).

## Keep the feedback loop short

- Run only the affected spec projects while iterating; run everything before you push.
- Keep slow suites (containers, databases, browsers) in their own projects so unit specifications stay quick.
  See [Integration specifications](./integration-specifications.md).
- Shard the suite across CI jobs once a single job becomes the bottleneck.
- Keep benchmarks out of pull request CI. This repository's benchmark workflow runs on demand
  (`workflow_dispatch`), never on pull requests, so the feedback loop stays short. Run them when you change
  runner or lifecycle code, or when you compare packages on your own suite:

```bash
Benchmarks/run.sh --n 5 --f 2 --r 1   # smoke test first
Benchmarks/run.sh --flavors xunit-v2,xunit-v3-mtp,cratis-runner
```

Run benchmarks on an idle machine; load, power state and caches change the results.
