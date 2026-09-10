# Fundamentals — project context

The shared building blocks beneath the Cratis stack: concepts, serialization,
dependency injection, and type discovery — for .NET (`Cratis.Fundamentals`)
and TypeScript (`@cratis/fundamentals`). A framework/library repository, not
an event-sourced application: apply language and library conventions, not
application vertical-slice patterns.

## Layout

- `Source/` — the .NET packages (concepts, serialization, DI, type discovery)
- `Source/` TypeScript packages ship alongside as workspace packages
- `Documentation/` — published docs (rendered at cratis.io)

## Commands

```bash
dotnet build          # .NET packages
dotnet test           # specifications
yarn install && yarn build   # TypeScript packages (workspace root)
```

CI runs the .NET and JavaScript builds plus markdown verification; publishing
is label-driven (`major`/`minor`/`patch`) through the shared publish workflow.

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
