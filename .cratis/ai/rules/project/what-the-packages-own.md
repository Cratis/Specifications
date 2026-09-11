---
applyTo: "**/*"
---

## What the packages own

The `Establish`/`Because`/`should_` assertion model, the `for_<Type>`/
`when_<behavior>`/`and_<condition>` hierarchy, scenario semantics, and the
supporting messaging/DI plumbing the specs run on. Cratis repositories write
their specifications with these packages; changes here change what a passing
specification means everywhere — treat the public surface conservatively.
