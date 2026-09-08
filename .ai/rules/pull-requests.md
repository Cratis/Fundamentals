---
applyTo: "**/*"
---

# How to Do Pull Requests

PR descriptions serve two purposes: they help reviewers understand the change *now*, and they become the release notes that users read *later*. Write them with both audiences in mind.

## Description

- Follow the repository's pull request template (`.github/pull_request_template.md`), when present.
- Focus on the **Added**, **Changed**, **Fixed**, **Removed**, **Security**, and **Deprecated** sections. Remove sections that are empty — don't leave blank headings.
- Each bullet should be short, self-contained, and release-note ready.
- **Write for users of the framework, not for internal developers.** Only include changes that have an impact on anyone using what we build — new APIs, changed behavior, fixed bugs, removed features. Do not list internal implementation details like storage changes, converter updates, gRPC contract internals, or spec additions. If a change is purely internal plumbing, it does not belong in the PR description.
- Add the associated issue reference at the end of a bullet when there is a real GitHub issue for the change (e.g. `(#351)`). If there is no associated issue, omit the reference entirely. Never use a placeholder like `(#issue)` or leave the example number `(#123)` literally, and never invent a random issue number. **Always verify the issue number using the `search_issues` or `list_issues` GitHub MCP tool — never guess or invent a number.**
- Include a summary only if there is a cohesive theme across the changes. If you find yourself restating individual bullets in slightly different words, the summary adds no value — remove it.
- Never include Copilot prompt content in the PR description. Remove any "Original prompt" / coding agent transcript blocks before publishing.

## Commits

See the full [Git Commits guide](./git-commits.md) for rules on logical grouping, message format, and staging discipline.

Quick reminders:
- Imperative mood: "Add author registration" not "Added author registration".
- Each commit = one logical unit of work. No WIP commits in the final PR.
- Never mix unrelated changes in a single commit.

## Labels

Confirm the current repository workflow contract before selecting release intent. Label mutations, merge, and any resulting publication/release require separate explicit authorization for their exact effects; a descriptive label does not grant authority.

- Label the PR according to semantic versioning impact:
  - **major** — breaking changes to public APIs
  - **minor** — new features, new slices, non-breaking additions
  - **patch** — bug fixes, refactoring with identical behavior

## Merge Method

- **Never squash-merge a pull request.** Squash merging destroys the PR's commit history and is prohibited without exception.
- Default to a normal merge commit: GitHub's **Create a merge commit** option, `merge_method: merge` in the GitHub API/MCP tool, or `gh pr merge --merge` in the CLI.
- Immediately before merging, verify the selected method. Never use `--squash` or `merge_method: squash`.
- Never rebase-merge. If normal merge commits are unavailable, stop and report the blocker; use additive fixes, never rewrite history.
- If the allowed merge method cannot be verified, stop and do not merge. Fix poor commits on the branch before merging; never destroy PR history at merge time.

## Quality Gates

Documentation-only changes use repository-supported non-release intent, ordinarily `no-release`; confirm the workflow contract rather than assuming a label or API state. Run relevant content, link, frontmatter, and corpus checks instead of unrelated application builds, and satisfy every repository-required check, including release-intent checks where supported. Documentation is never a blanket exemption from red CI.

Before marking code/automation work ready, select the affected-project gates that apply from the following list; run wider checks for cross-cutting changes and repository-required merge/release gates:
- `dotnet build` — zero errors, zero warnings
- `dotnet test` — all specs pass
- `yarn lint` — zero errors
- `npx tsc -b` — zero TypeScript errors
- Code follows all project coding standards and conventions
- **Required CI checks pass.** After an authorized push, inspect checks and failure logs. Diagnose within a bounded attempt, fix in-scope causes, and re-run relevant gates after each fix. Report unrelated/environmental failures and missing authority as blockers rather than retrying indefinitely or silently waiving required checks.
