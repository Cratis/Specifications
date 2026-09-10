# Specifications — project context

Cratis Specifications: specification by example (BDD) for .NET — Given/When/Then
specs with xUnit and NUnit, in the style of Machine.Specifications. Published
as `Cratis.Specifications.XUnit` and `Cratis.Specifications.NUnit` on NuGet.
A framework repository, not an event-sourced application.

## What the packages own

The `Establish`/`Because`/`should_` assertion model, the `for_<Type>`/
`when_<behavior>`/`and_<condition>` hierarchy, scenario semantics, and the
supporting messaging/DI plumbing the specs run on. Cratis repositories write
their specifications with these packages; changes here change what a passing
specification means everywhere — treat the public surface conservatively.

## Commands

```bash
dotnet build
dotnet test
```

## Conventions

- The specification vocabulary (`Establish`, `Because`, `should_`, `for_`,
  `when_`) is the product; keep naming stable across the xUnit and NUnit
  flavors.
- Releasing is a labeled merge to `main` (`major`/`minor`/`patch`) through
  the shared publish workflow.

## AI-assisted development

This repository uses the Cratis AI contract:

- **`.cratis/ai.json`** records the subscription — `cratis/documentation` plus the `cratis/engineering/csharp` maintainer cell.
- **`.cratis/PROJECT.md`** (this file) is the canonical project context; the root `AGENTS.md`, `CLAUDE.md`, and `GEMINI.md` are minimal bootstraps that point here and do nothing else.
- There is **no local AI corpus and no generated tool adapters** in this repository. Shared skills arrive through the Cratis AI marketplace plugins (Claude Code, Codex, GitHub Copilot, Cursor, and Pi are installable today — see the [harness guide](https://www.cratis.io/ai/harnesses/)).

For contributors:

1. Install the Cratis plugin for your harness once (per the harness guide); the subscribed profiles' skills then load automatically when tasks match.
2. General, reusable improvements are proposed in [`Cratis/AI`](https://github.com/Cratis/AI) — never copied into, or synchronized from, this repository.
3. Repository-specific facts and conventions belong in this file; repository-local skills live under `.agents/skills/`.
4. AI session work records (plans, handovers, session notes, scratch analyses) stay in the untracked `.ai-work/` folder and never enter git; a durable follow-up becomes a GitHub issue.
