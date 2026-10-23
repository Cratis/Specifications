# The native runner

`Cratis.Specifications.Runner` runs specifications directly on
[Microsoft.Testing.Platform](https://learn.microsoft.com/dotnet/core/testing/microsoft-testing-platform-intro) (MTP),
without xUnit, NUnit or TUnit underneath. A source generator registers the specification classes at compile time, so
discovery needs no reflection scan and the runner starts fast.

This guide covers where the runner works, how to run and filter it, what it cannot do, and how to move an existing
xUnit project to it. For the specification vocabulary itself, see the
[repository README](https://github.com/Cratis/Specifications/blob/main/README.md#native-runner-on-microsoft-testing-platform-experimental).

## Set up a project

Reference the package from a test project. It brings the Microsoft.Testing.Platform packages and the source generator
and makes the project an executable test application; no `Microsoft.NET.Test.Sdk` or runner adapter is needed.

```xml
<ItemGroup>
    <PackageReference Include="Cratis.Specifications.Runner" Version="5.0.0" />
</ItemGroup>
```

```bash
dotnet build --configuration Release
dotnet run --project Source/MyProject.Specs --configuration Release --no-build
```

## Supported IDEs and tools

Because the runner is a Microsoft.Testing.Platform application, it works wherever MTP is supported. This list comes
from the Microsoft and JetBrains documentation of MTP support; it has not been verified by hand in each IDE.

| Tool | Requirement |
| --- | --- |
| Visual Studio 2022 | Version 17.14 or later |
| Visual Studio 2026 | Supported |
| JetBrains Rider | Enable Testing Platform discovery in the test settings |
| Visual Studio Code | C# Dev Kit |
| Command line and CI | `dotnet run`, `dotnet test` on a .NET 10 SDK configured for Microsoft.Testing.Platform, or the built executable |

Test names read as sentences: `for_Author.when_registering.should_be_registered` is shown as
`Author › when registering › should be registered`. A test is identified by its namespace, class and method name,
including the return type of the method (`void`, `Task` or `ValueTask`), so results keep their history in the IDE.

## Command line options

| Option | Effect |
| --- | --- |
| `--treenode-filter '/<assembly>/<namespace>/<class>/<fact>'` | Selects facts by code name. Segments support the platform's wildcards and alternatives, for example `--treenode-filter '/*/*/when_registering/*'`. There are no trait properties. |
| `--filter-uid <uid>` | Selects a class or a fact by its UID. Other filter types fail explicitly instead of running an unfiltered suite. |
| `--specs-parallel <n>` | Maximum number of classes running at the same time. Defaults to `CRATIS_SPECS_PARALLEL` or the processor count; `1` runs the classes one by one. |
| `--specs-sequential-facts` | Runs the facts of a class one at a time instead of concurrently. |
| `--fail-fast` | Stops starting new classes after the first failure. |
| `--last-failed-first` | Starts the classes that failed in the previous run first. |
| `--shard <index>/<count>` | Runs one shard of the classes, for example `--shard 2/4`. |
| `--specs-cache <path\|none>` | Run cache of durations and failures; defaults to `.specs-cache` next to the test assembly. |
| `--report-trx` | Writes a TRX report (standard Microsoft.Testing.Platform option). |

Pass options after `--` with `dotnet run`: `dotnet run --project <project> --no-build -- --fail-fast`.

## Parallelism

Classes run concurrently, up to `--specs-parallel`. Within a class `Establish` and `Because` run once, the facts may run
concurrently and must only read the shared context, then `Destroy` and disposal run once. The runner orders the classes so the run finishes as early as possible, using the durations in the run cache, and
starts previously failed classes first with `--last-failed-first`.

## Cancellation and stopping

Cancelling the run (for example Ctrl+C, or Stop in the IDE) and `--fail-fast` both stop scheduling:

- No new class is started after the stop.
- A class that is already running finishes: its facts, `Destroy` and disposal always run.
- Facts of classes that never started are reported as skipped, so every selected fact ends in a terminal state.
- Running code can cooperate through `SpecificationContext.CancellationToken`, which flows across awaits and is
  isolated per class. Cancellation never interrupts running code forcibly.

## Limits

- **Methods must return `void`, `Task` or `ValueTask`.** A fact with another return type fails its class with a
  discovery error. An `async void` fact or lifecycle method is a compile error (`CRSPEC0010`), because its failures can
  not be awaited.
- **Facts and lifecycle methods are parameterless instance methods.** Generic methods and static methods are not facts.
- **Classes must be non-generic, non-abstract and accessible to the test assembly.** Abstract and generic classes are
  not run themselves; their members run as part of the classes that derive from them.
- **Reflection fallback.** Methods are called through generated code; private and protected methods through
  `UnsafeAccessor`, so there is no reflection at run time. The exception is a method that the test assembly cannot call
  and that is declared on a constructed generic base type (for example `a_context<int>`): it is called through
  reflection, which is not trim or AOT safe. The generator warns with `CRSPEC0011`; make the method `public` or
  `internal` to avoid it.
- **Native AOT.** Specifications that avoid the reflection fallback need no reflection at run time, which is what makes
  them compatible with trimming and ahead-of-time compilation.
- **No test traits or categories.** Select tests with the tree filter, UIDs or sharding.

## Migrating from xUnit

1. Replace the `Cratis.Specifications.XUnit` package, `xunit`, `xunit.runner.visualstudio` and
   `Microsoft.NET.Test.Sdk` with `Cratis.Specifications.Runner`.
2. Remove `[Fact]` attributes. Methods named `should_*` are facts without an attribute (`[Fact]` is still accepted, also
   with `Skip`).
3. Keep `Establish`/`Because`/`Destroy` (or `Given`/`When`). The lifecycle matches xUnit's shared lifecycle: one
   instance per class, `Establish` and `Because` once.
4. Keep the assertions. `Should*` assertions are part of the specifications packages and throw
   `SpecificationAssertionException` instead of `XunitException`.
5. Replace `dotnet test --filter` expressions with `--treenode-filter` or `--filter-uid`, and the xUnit run parameters
   with the options above.
6. Run it with `dotnet run` or `dotnet test` on a .NET 10 SDK configured for Microsoft.Testing.Platform.
