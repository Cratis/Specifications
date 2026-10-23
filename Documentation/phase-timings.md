# Phase timings

Phase timings show where the time of a specification run goes: how long Establish, Because and Destroy take for each
specification class, and (in the native runner) each fact. They are off by default and cost nothing while off: no
observer is registered, so the lifecycle runs exactly as it does without the feature.

## Turn them on

Set the environment variable, or set the MSBuild property in the test project:

```bash
CRATIS_SPECS_TIMINGS=1 dotnet test
```

```xml
<PropertyGroup>
    <CratisSpecificationsPhaseTimings>true</CratisSpecificationsPhaseTimings>
    <!-- Optional: the duration in milliseconds above which a phase is reported. The default is 1000. -->
    <CratisSpecificationsSlowPhaseThreshold>500</CratisSpecificationsSlowPhaseThreshold>
</PropertyGroup>
```

The property is written to the test assembly as `AssemblyMetadata` and read once when the package loads.

## Slow phases

A phase that takes longer than the threshold is reported through the native channel of each framework:

| Package | Where the line appears |
| --- | --- |
| `Cratis.Specifications.XUnit.v3` | An xUnit diagnostic message, and the test output of the running test. |
| `Cratis.Specifications.XUnit` (v2) | A diagnostic message. xUnit v2 shows it only with `diagnosticMessages` enabled, and only with the opt-in `SpecificationTestFramework`. |
| `Cratis.Specifications.NUnit`, `.NUnit5` | `TestContext.Progress`. |
| `Cratis.Specifications.TUnit` | The output of the running test. |
| `Cratis.Specifications.Runner` | The `TimingProperty` of each fact node: one `StepTimingInfo` for Establish, Because, Destroy and the fact itself, which a TRX report carries. |

A line looks like `Cratis.Specifications: slow Establish (spec) took 1520.4 ms in Orders › when placing an order`.
It contains only names and durations, never exception messages.

## The JSON timings file

Set `CRATIS_SPECS_TIMINGS_FILE` (or the `CratisSpecificationsPhaseTimingsFile` assembly metadata) to write every phase
to a JSON file when the test session ends. A failure to write the file is reported without its message and never fails
the run.

```json
{
  "schema": 1,
  "phases": [
    {
      "phase": "Establish",
      "owner": "spec",
      "specification": "Orders › when placing an order",
      "startedUtc": "2026-01-02T03:04:05+00:00",
      "durationMs": 12.5,
      "failed": false
    }
  ]
}
```

`owner` is `session`, `key` or `spec`; `resourceKey` is present for a phase of a shared resource. The facts of a class
are reported as phases by the native runner only; the other frameworks time their facts themselves.

## What they do not do

Timing output is additive: it never changes whether a specification passes, and an unavailable output channel is
ignored. OpenTelemetry export and per-assertion timings are not part of 5.0.
