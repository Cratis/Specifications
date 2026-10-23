# Guides

Task-oriented guides for getting the most out of Cratis Specifications. The
[repository README](https://github.com/Cratis/Specifications/blob/main/README.md) is the reference for
packages, lifecycle conventions, assertions and analyzers; these guides assume you have read its
[setup section](https://github.com/Cratis/Specifications/blob/main/README.md#setting-up-a-project).

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
