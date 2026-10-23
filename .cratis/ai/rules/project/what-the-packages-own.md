---
applyTo: "**/*"
---

## What the packages own

The `Establish`/`Because`/`should_` assertion model, the `for_<Type>`/
`when_<behavior>`/`and_<condition>` hierarchy, scenario semantics, and the
supporting messaging/DI plumbing the specs run on. Cratis repositories write
their specifications with these packages; changes here change what a passing
specification means everywhere — treat the public surface conservatively.

## What 5.0 added to that surface

- **Assertions** (`Should*` and `SpecificationAssertionException`) live in `Cratis.Specifications`; each framework
  package forwards them and supplies the exception type its framework expects. Behavior and messages are the same
  in every package.
- **Lifecycle defaults:** xUnit shares the specification instance per class and every package shows readable names,
  both switched off with `CratisSpecificationsSharedLifecycle=false` and `CratisSpecificationsReadableNames=false`.
- **Analyzers** `CRSPEC0001`–`CRSPEC0009` ship only in `Cratis.Specifications`; the Runner's generator adds
  `CRSPEC0010` and `CRSPEC0011`. `CRSPEC0008` is an error. `CratisSpecificationsAnalyzers=false|ide` controls them.
- **Experimental surface:** shared resources and isolation leases (`CRSPECEXP01`, opt in with
  `CratisSpecificationsSharedResources=true`). Treat it as changeable until it graduates.
- Packages target `net10.0` only. Keep `Cratis.Specifications` and the framework package on the same version.
- Benchmarks (`Benchmarks/`) are a local tool; they are never part of pull request CI or the release.
