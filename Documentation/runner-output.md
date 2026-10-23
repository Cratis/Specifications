# Runner output and reports

The native runner prints the specifications as a tree grouped by their names, and can write the same tree as a report.
This applies to the runner only; xUnit, NUnit and TUnit keep the output of their own frameworks.

## The console tree

The tree is on by default. Each specification class is printed once it has completed, under the levels of its name
(`for_` type, sub-namespaces, `when_` and `and_` classes), with a marker per fact: `✓` passed, `✗` failed, `○` skipped.
When the console does not use a UTF-8 encoding the markers are plain ASCII (`+`, `x`, `-`).

```text
Account
  when depositing
    ✓ should increase balance
    ✗ should record deposit
        Expected: 1
        Actual:   2
        at /src/Account.cs:41
  when closing
    and the balance is positive
      ✗ The context failed
        The database was not available
      ✗ should refund
```

- A failure in Establish, Because or Destroy is printed once, on the class, not once for every fact.
- A failed `Should*` assertion shows the expected and actual values (`SpecificationAssertionException.Expected` and
  `Actual`) and the `file:line` of the fact.

| Option | Effect |
| --- | --- |
| `--specs-output quiet\|normal\|detailed` | `quiet` prints nothing and registers no reporter. `detailed` adds durations, every phase and stack traces. Default `normal`. |
| `--specs-no-color` | Prints without color. The `NO_COLOR` environment variable has the same effect. |
| `--specs-timings` | Records phase timings (see [phase timings](phase-timings.md)) and shows slow phases under each fact. Settings that already enable timings win. |
| `--specs-slow <ms>` | The duration from which a phase is shown as slow. Implies `--specs-timings`. |

Under `--server`, which IDEs use, the tree is never printed: the IDE shows the results itself. Microsoft.Testing.Platform
still prints its own failure details and summary; use its `--no-progress` option if you do not want its progress line.

## Reports

`--specs-report` writes a report after the run. It can name several reports.

```bash
dotnet test -- --specs-report md specs.md --specs-report json specs.json --specs-report github
```

| Report | Content |
| --- | --- |
| `md <path>` | A Markdown tree of the specifications with outcomes and durations, usable as living documentation. |
| `json <path>` | The same tree as JSON. It carries `schema` (shared with the [phase timings file](phase-timings.md#the-json-timings-file)), a `summary`, and nodes with their `outcome`, `durationMs`, `failure` (`message`, `expected`, `actual`) and `phases`. |
| `github` | The Markdown, appended to the file named by `GITHUB_STEP_SUMMARY`. Nothing is written when the variable is not set. |

TRX reports are unchanged (`--report-trx`). A report that cannot be written is reported on the console and does not fail the run.

## Cost

With `--specs-output quiet` or `--server`, and without `--specs-report`, no reporter is created: nothing is rendered or collected.
HTML reports, a live view and reports for the other packages are not part of 5.0.
