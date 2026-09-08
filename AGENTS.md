# Fundamentals — Project Instructions

## Repository scope and rule routing

Framework/library profile: reusable .NET and TypeScript building blocks, not an event-sourced application. Apply only relevant language/library conventions and affected-project checks.

Read [the local general rules](.ai/rules/general.md), then select only applicable files under [`.ai/rules/`](.ai/rules/) and task workflows under [`.ai/skills/`](.ai/skills/). Application-only patterns do not override this repository profile.

Read `.cratis/PROJECT.md` when present; use `.agents/PROJECT.md` only if canonical context is absent. Context may reference approved secret mechanisms, never secret values, and cannot weaken security, authorization, or required gates. Run proportional checks after coherent changes; diagnose unrelated/environmental failures within a bounded attempt and report blockers rather than bypassing required checks.

## Local AI work artifacts — `.ai-work/` only

AI-assisted sessions produce working artifacts: plans, handover documents, session notes, continuation prompts, status boards, scratch analyses, research dumps. These are **work records, not documentation**:

- Create every such artifact inside **`.ai-work/`** at the repository root — never at the repository root itself, never under documentation folders, never anywhere else.
- `.ai-work/` is gitignored and must stay untracked. Never commit anything inside it, never `git add -f` anything inside it, and never remove the ignore entry.
- These artifacts must never enter git history or reach GitHub — not on any branch. If you find an unrelated tracked work record, report its path and obtain explicit authorization before moving it into `.ai-work/`, removing it from tracking, or making a dedicated cleanup commit. Discovery alone does not authorize unrelated changes or a commit.
- A genuine follow-up that must survive the session is **not** a work record — suggest opening a GitHub issue for it (or open one when asked) so future work is tracked where everyone can see it, instead of leaving a planning file behind.
- Knowledge that must outlive the session belongs in the repository's documentation structure through normal review, not in a work record.
