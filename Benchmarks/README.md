# Test framework benchmarks

A repeatable end-to-end comparison of Cratis Specifications on xUnit v2, xUnit v3,
NUnit 4 and TUnit, alongside plain-framework baselines. NUnit 5 is included only
when `Source/NUnit5/NUnit5.csproj` exists. Nothing is packable or included in
`Specifications.sln`; the normal solution build and CI are unchanged.

## Run

Requires Python 3 (standard library only), .NET SDK 10, and `/usr/bin/time`
(macOS or GNU time on Linux). No Python packages are installed.

```bash
# Always smoke-test counts first (10 passing tests per suite).
Benchmarks/run.sh --n 5 --f 2 --r 1

# Default: 500 classes, 5 facts, 5 measured repetitions, both parallel modes.
Benchmarks/run.sh

# Shared-machine example; select a subset to bound each execution phase.
N=300 F=5 R=5 Benchmarks/run.sh --flavors xunit-v2
Benchmarks/run.sh --n 300 --f 5 --r 5 --flavors xunit-v3,nunit4,tunit

# Generator alone, for inspection or manual experiments.
python3 Benchmarks/generate.py --n 5 --f 2
python3 -m unittest discover -s Benchmarks -p 'test_*.py' -v
```

`--n`, `--f`, `--r`, `--flavors`, `--parallel`, `--work-us` and `--aot` override `N`, `F`, `R`,
`FLAVORS`, `PARALLEL`, `WORK_US` and `AOT=1`. `--work-us <µs>` adds deterministic CPU work
to every context, half in Establish and half in Because (and makes every Because asynchronous),
so the cost of running a context once per class versus once per fact shows. `--aot` also
publishes the `cratis-runner` suite with Native AOT and measures the native executable;
the publish is timed separately and not included in the build columns. Numbers must be positive; flavors are comma-separated
with no duplicates. `--parallel` accepts `default`, `off`, or `both` (default).
Run only one harness process per checkout: generation and runner configuration
share files. On shared developer machines, run each flavor through your build/test
admission queue, not concurrently.

`generated/<flavor>/` and `generated/<flavor>-raw/` contain a single-TFM net10.0
project and deterministic source. Generation overwrites only its own two files;
it never removes directories. Cratis projects reference `../../../Source/...`,
so rebuilding picks up library changes. Raw projects have no Cratis reference.
Package versions come from the repository's central package configuration;
NUnit references preserve any version override from the corresponding source project.
Generated source is marked auto-generated to exclude naming/style diagnostics;
framework/source-generator diagnostics and build warnings still fail Release builds.

## Workload and lifecycle

Every suite has N concrete classes and F assertion methods. Establish allocates
an array, a list and a description string; Because sums the items, appends a
result and normalizes the description. Every third class awaits one `Task.Yield`. Every fourth class inherits a
two-level `given` context chain. Assertions rotate through five checks of the
computed values (additional facts repeat that rotation). Facts are read-only.
Cratis uses `ShouldEqual`; raw xUnit/NUnit use the corresponding native assertion.
Raw TUnit uses synchronous equality checks throwing on failure, matching the
synchronous checks behind Cratis TUnit's assertions rather than introducing an
async assertion workload.

Cratis xUnit v2 and v3 explicitly select `SpecificationTestFramework` (v3 with the
type-based assembly attribute) to benchmark shared-context, once-per-class
specifications, matching Cratis NUnit and TUnit. The raw xUnit baselines use native
`IAsyncLifetime` setup **per fact**, NUnit uses `OneTimeSetUp`, and raw TUnit uses
`Before(Test)` with native per-test instances. Raw given contexts initialize in
base constructors; Cratis discovers `Establish` methods by convention. This is
intentionally a comparison of natural usage, **not** an exact subtraction of Cratis
reflection overhead: setup frequency/instance reuse differ. No lifecycle policy
in the shipped libraries is changed for this benchmark.

## Measurements

For each project the harness performs an untimed restore, a timed Release
`Rebuild`, then a timed no-op incremental build. Rebuild recompiles referenced
projects too; it does not flush NuGet/OS caches. Subsequent projects may benefit
from warmed dependencies. Build times are recorded once and reused in each
runner/mode table row, not remeasured for each row.

Each runner/mode gets one validated, excluded warm-up and R fresh processes.
The report includes wall-time median/min/max and maximum sampled peak RSS:

| Flavor | Primary path | Additional path |
|---|---|---|
| xUnit v2 | `dotnet test` / VSTest | None |
| xUnit v3 (`xunit.v3.mtp-off`) | `dotnet test` / VSTest | Built executable / native xUnit runner |
| NUnit 4/5 | `dotnet test` / NUnit3TestAdapter | None |
| TUnit | `dotnet run --no-build --no-restore` / MTP | Built executable / MTP |
| Cratis runner (`cratis-runner`) | `dotnet run --no-build --no-restore` / MTP | Built executable / MTP; Native AOT executable with `--aot` |

All `dotnet test` invocations also use `--no-build --no-restore`. TUnit is **not**
run via `dotnet test`: this repository uses VSTest globally and mixing MTP would
require changing `global.json`. Direct paths run the generated apphost, not an
MSBuild invocation. Warm-up does not retain JIT code across measured processes;
startup/JIT/discovery/report-writing overhead remains part of every measurement.

`/usr/bin/time -l` (macOS) or `-v` (Linux) measures wall time externally and peak
RSS. macOS RSS bytes and Linux KiB are normalized to MiB. RSS includes the timed
runner/descendant accounting supplied by the OS; it is **not** aggregate concurrent
process-tree memory, and is not directly comparable across operating systems.

Every warm-up and sample must exit zero, report exactly N×F total **and passed**
tests, with zero failures/skips, and provide RSS. Counts come from fresh TRX files
(VSTest/MTP) or xUnit XML (direct v3), never console regexes. Missing/incorrect
reports invalidate that row and the harness exits nonzero; independent rows still
run. There are no automatic retries. Diagnose the retained logs; after two manual
attempts at an unrunnable flavor, omit it from the next subset and report it.

## Parallelism

Both modes run by default; never compare rows from different modes as if they
were controlled for parallelism.

- xUnit: default runner settings (collections/classes may run concurrently).
  Disabled via output `xunit.runner.json` (`parallelizeTestCollections: false`,
  `maxParallelThreads: 1`); direct v3 also passes `-parallelMode none`.
- NUnit: framework default, **not parallelized** because no `Parallelizable`
  attribute is generated. Disabled explicitly with `NUnit.NumberOfTestWorkers=0`.
  The default/off comparison can therefore be effectively identical.
- TUnit: framework's default concurrency; disabled with
  `--maximum-parallel-tests 1` for both launch paths.
- Cratis runner: classes on `ProcessorCount` workers, asynchronous facts of one class
  concurrently; disabled with `--specs-parallel 1 --specs-sequential-facts`. It has no
  raw baseline (there is no plain framework underneath).

## Output and interpretation

`results/<UTC timestamp>-<host>.md` contains the table; the sibling `.json`
contains configuration, exact commands, build measurements, warm-ups and every
sample. The sibling `-logs/` directory retains runner reports, stdout/stderr and
time statistics. Generated files and results are gitignored; no machine-specific
ranking is committed. Load averages (`uptime`), SDK, host and OS are recorded.

The manual-only GitHub Actions workflow accepts N/F/R, uses Ubuntu with SDK 10,
runs both modes, uploads the reports/logs and puts the table in the job summary.
A failed row fails the job without hiding measurements for successful rows.

Machine load, power/thermal state, CPU count, JIT, file caches, execution order,
report formats and framework default parallelism all affect results. Suites run
sequentially (Cratis then raw for each flavor), not randomized; rerun on an idle,
fixed machine for a meaningful ranking. Prefer same-runner, same-mode comparisons;
report both native/direct speed and CLI-inclusive speed rather than declaring
an unconditional fastest framework. Tiny suites mostly measure runner startup.
