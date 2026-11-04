# Guides

Start with a single behavior in Cratis Specifications, then use these guides as your suite grows. The
[repository README](https://github.com/Cratis/Specifications/blob/main/README.md) is the reference for
packages, lifecycle conventions, assertions and analyzers; these guides assume you have read its
[setup section](https://github.com/Cratis/Specifications/blob/main/README.md#setting-up-a-project).

After setup, start with [Level 1 of Structuring specifications](./structuring-specifications.md#level-1-a-single-behavior).
The example uses xUnit; work through the later levels only as your behavior needs them.

You can keep the runner you chose during setup. [The native runner](./runner.md) is an optional alternative;
read its [limits](./runner.md#limits) before deciding to migrate from xUnit.

| Guide | Use it to |
| --- | --- |
| [Running specifications fast](./running-fast.md) | Pick a package and runner for speed, share the lifecycle, filter, shard and keep the feedback loop short. |
| [Structuring specifications](./structuring-specifications.md) | Grow a specification from one behavior to shared contexts, doubles, async work and readable names, in six levels. |
| [Integration specifications](./integration-specifications.md) | Write specifications that need real infrastructure and keep them fast and reliable. |
| [The native runner](./runner.md) | Run specifications on Microsoft.Testing.Platform: IDEs, options, limits and migrating from xUnit. |
| [Runner output and reports](./runner-output.md) | Read the console tree and write Markdown, JSON and GitHub summary reports. |
| [Phase timings](./phase-timings.md) | See where the time of a run goes and report slow phases. |
| [Shared resources](./shared-resources.md) | Share an expensive resource across specifications and lease an isolated part of it (experimental). |

Upgrading from 4.x? See the [5.0 upgrade guide](https://github.com/Cratis/Specifications/blob/main/Documentation/upgrading-5.0.md).
