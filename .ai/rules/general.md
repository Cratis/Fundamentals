# GitHub Copilot Instructions

## Project Philosophy

Cratis builds tools for event-sourced systems with a focus on **ease of use**, **productivity**, and **maintainability**. Every rule in these instructions serves one or more of these core values:

- **Lovable APIs** — APIs should be pleasant to use. Provide sane defaults, make them flexible, extensible, and overridable. If an API feels awkward, it is wrong.
- **Easy to do things right, hard to do things wrong** — Convention over configuration. Artifact discovery by naming. Minimal boilerplate. The framework should guide developers into the pit of success.
- **Events are facts** — Immutable records of things that happened. Never nullable, never ambiguous, never multipurpose. If you find yourself adding a nullable property to an event, you need a second event.
- **High cohesion through vertical slices** — Everything for a behavior lives together: backend, frontend, specs. Navigate by feature, not by technical layer. A developer working on "author registration" should never need to jump between `Commands/`, `Handlers/`, and `Events/` folders.
- **Full-stack type safety** — Shared models flow from C# through proxy generation to TypeScript. End-to-end typing without manual synchronization.
- **Specialization over reuse** — Build focused, purpose-specific projections and read models rather than reusing one model across conflicting scenarios. Dedicated models are easier to maintain, perform better, and never break unrelated features.
- **Consistency is king** — When in doubt, follow the established pattern. Consistency across the codebase trumps local optimization. A slightly less elegant solution that matches the rest of the codebase is better than a clever one that stands out.

When these instructions don't explicitly cover a situation, apply these values to make a judgment call.

## General

- **Always use American English spelling** in all code, comments, documentation, and XML docs — no exceptions.
  - `-ize` not `-ise`: initialize, serialize, customize, normalize, organize, authorize, specialize, centralize, utilize
  - `-or` not `-our`: behavior, color, favor, honor, humor, neighbor, flavor
  - `-ization` not `-isation`: initialization, serialization, customization, normalization, organization, authorization
  - `-er` not `-re`: center, fiber, meter
  - `-og` not `-ogue`: dialog, catalog, analog
  - `-ling` not `-lling`: modeling, signaling, labeling, canceling
  - `-ense` not `-ence`: license, defense, offense
  - `-ment` not `-ement`: judgment, acknowledgment
  - Other: gray (not grey), program (not programme), fulfill (not fulfil), enroll (not enrol)
  - When in doubt, use the US spelling — check a US dictionary.
- Write clear and concise comments for each function.
- Make only high confidence suggestions when reviewing code changes.
- Never change global.json unless explicitly asked to.
- Never change package.json or package-lock.json files unless explicitly asked to.
- Never change NuGet.config files unless explicitly asked to.
- Always ensure that the code compiles without warnings.
- Always treat warnings as errors and fix them before considering the work complete.
- Ensure the relevant tests for affected code pass; wider checks apply when required by scope or repository merge/release gates.
- Always ensure that the code adheres to the project's coding standards.
- Always ensure that the code is maintainable.
- For PR descriptions, use short release-note bullets that focus on **user-facing impact only** — new APIs, changed behavior, fixed bugs. Do not include internal implementation details (storage changes, converter updates, gRPC internals, spec additions). Append the **actual** issue number only when the PR is associated with a real GitHub issue (for example `(#351)`). If there is no associated issue, omit the reference entirely. Never use placeholder text like `(#issue)`, never leave the literal example `(#123)`, and never invent a random issue number. Never include Copilot "Original prompt" blocks. **Always verify the issue number using the `search_issues` or `list_issues` GitHub MCP tool — never guess or invent a number.**
- Always reuse the active terminal for commands.
- Do not create new terminals unless current one is busy or fails.
- When asked to commit, push, create a PR, ship, or land changes, always use the **ship-changes** skill.
- **Never squash-merge a pull request.** Default to a normal merge commit that preserves every PR commit. Never use a rebase merge; if normal merge commits are unavailable, stop and report the blocker.

## Development Workflow

- After a coherent set of changes, run the affected project’s incremental build/compile and targeted regression checks, not a build after every file.
- Before adding parameters to interfaces or function signatures, review all usages to ensure the new parameter is needed at every call site.
- When modifying imports, audit all occurrences — verify additions are used and removals don't break other files.
- Before concluding code work, run relevant affected-project specs/tests; after a fix, re-run its failed gate. Diagnose unrelated or environmental failures within a bounded attempt and report blockers, not endless retries.
- Run affected-project incremental checks after a coherent change, then targeted regression tests for the changed behavior. Re-run a failed gate after a relevant fix. Reserve wider matrices and clean/Release builds for cross-cutting changes, demonstrated stale outputs, or required merge/release gates. Documentation/rule-only edits need relevant Markdown, frontmatter, link, and corpus checks, not an application build. Diagnose unrelated or environmental failures within a bounded attempt; report the evidence and blocker instead of broadening scope or retrying indefinitely. Required gates remain blocking until satisfied; never silently waive red CI.
- After an authorized push, inspect required CI checks. Diagnose failures within a bounded attempt, re-run relevant gates after in-scope fixes, and report unrelated or environmental failures as blockers. Further commits/pushes require the requested scope; required red CI is not waived.

## Definition of Done

Work is not done until all applicable items below are complete:

- The affected solution or project builds successfully with zero warnings and zero errors.
- Relevant specs/tests for every affected project pass.
- For public-facing changes (clients, SDKs, public APIs, developer-facing behavior), associated documentation is added or updated.
- Run the repository’s existing documentation/corpus checks for changed content where available; report missing tooling. Documentation/rule-only edits do not require application builds.

## Detailed Guides

These guides contain the full rules, examples, and rationale for each topic. The sections above are the global defaults; the guides go deeper into each area:
   - [Code Quality](./code-quality.md)
   - [Code Quality — C#](./code-quality.csharp.md)
   - [Code Quality — TypeScript](./code-quality.typescript.md)
   - [C# Conventions](./csharp.md)
   - [How to Write Specs](./specs.md)
   - [How to Write C# Specs](./specs.csharp.md)
   - [How to Write TypeScript Specs](./specs.typescript.md)
   - [Entity Framework Core](./efcore.md)
   - [Entity Framework Core Specs](./efcore.specs.md)
   - [Concepts (ConceptAs)](./concepts.md)
   - [Documentation](./documentation.md)
   - [Pull Requests](./pull-requests.md)
   - [Vertical Slices](./vertical-slices.md)
   - [TypeScript Conventions](./typescript.md)
   - [React Components](./components.md)
   - [Dialogs](./dialogs.md)
   - [Reactors](./reactors.md)
   - [Orleans](./orleans.md)

## Header

All files should start with the following header:

```csharp
// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
```
