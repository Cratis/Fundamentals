---
name: ship-changes
description: >
  Use when asked to commit, push, create a PR, ship, or land changes. Stop at
  the requested verb; separately authorize merge, publication, issue effects,
  label mutations, and branch deletion. Preserve history and explicit staging.

---

# Ship Changes

This skill handles only the requested shipping endpoint. Preserve repository-specific
conventions and stricter private/effect gates; do not infer authority for later steps.

## Authorization and stopping points

The requested verb is the stopping point, not permission for the whole workflow:

- **Commit-only:** review, explicitly stage authorized paths, commit, and stop. Do not push or open a PR.
- **Push-only:** push the authorized branch/commits and stop. Do not create additional commits or a PR unless requested.
- **PR-only:** prepare/open the requested PR and report required checks; stop before merge.
- **Ship/land:** clarify the exact intended endpoint and effects. These words alone do not authorize destructive or notification-bearing effects.

Merge, issue comments or closure, label mutations (especially labels that trigger publication/releases), publication, and local or remote branch deletion each require separate explicit authorization for exact targets and effects. Apply the repository's current mutation protocol and stricter local/private gates; tool access and inverse escrow alone supply no authority. For destructive/bulk effects, prepare an exact dry-run, capture pre-state and deterministic inverse escrow in ignored `.ai-work/`, obtain approval, recheck preconditions, and record/read back outcomes through the approved repository-owned adapter. If a required adapter, safe inverse/compensation, or authorization is missing, stop. Preserve any stricter prohibition below.

Never rewrite history: no amend, rebase, squash merge, hard reset, force-push, or forced branch deletion. Use new commits, revert, cherry-pick, and merge instead. A request to ship does not override this prohibition.

## Inputs

Collect the following before starting:

- **Release intent** — propose `major`, `minor`, `patch`, or repository-supported non-release intent (ordinarily `no-release`) from the actual impact; confirm the current workflow contract. Apply a label only with explicit authorization for that exact label and any publication effects. If the user forbids labels but the repository requires one, report the blocker; do not silently omit it or bypass the gate.
- **Branch name suffix** — short kebab-case description of the work, e.g. `fix/testing-orleans-runtime-assemblies`. Determine from the nature of the changes if not provided.
- **Related GitHub issue** — search GitHub issues if the change likely relates to one; use the real number or omit the reference when none exists. Never invent or reuse example numbers.

## Step 1 — Review the working tree

```
git status
git diff
```

Read the full diff. Understand every changed file before touching git.
Do **not** start staging until you know exactly how the commits will be split.

## Step 2 — Create the branch

Branch off of current `main`. Always use a prefix:

| Prefix | When to use |
|--------|-------------|
| `fix/` | bug fixes, runtime errors, incorrect behavior |
| `feat/` | new features, new slices, new capabilities |
| `chore/` | build infra, tooling, docs, refactoring |

```
git checkout -b <prefix>/<short-description>
```

## Step 3 — Make logical commits

### Commit splitting rules

Split commits so that each one is a single logical unit of work:

1. **Infrastructure / plumbing first** — new types, interfaces, MSBuild targets, shared build props — anything that later commits build on.
2. **Core behavior second** — the actual fix or feature that uses the infrastructure.
3. **Specs / tests third** — only when specs are clearly separate from the behavior change (e.g. new integration spec added after the source fix). Combine with behavior commit when tightly coupled.
4. **Integration or wiring last** — DI registration, routing, UI hookup.

Never mix unrelated changes in a single commit.

### Staging discipline

Stage files explicitly — never `git add .` or `git add -A`:

```
git add <file1> <file2>
git diff --cached          # verify staged content before committing
git commit -m "<message>"
```

### Commit message format

```
<imperative summary — 72-char max, no trailing period>

<optional body: WHY the change was made, context, trade-offs>
<use bullets for multi-part changes>
```

- Subject starts with a verb: `Add`, `Fix`, `Remove`, `Rename`, `Extract`, `Update`, `Support`.
- Body separated from subject by a blank line.
- Body explains *why*, not *what* — the diff shows the what.

**Good examples:**

```
Add _PackPrivateAssemblyGlobs target for runtime-only NuGet package embedding

Extend the shared client build infrastructure with a new MSBuild target.
The new PrivatePackageAssemblyGlob item type globs $(OutputPath) at pack
time and embeds matching DLLs into lib/{tfm}/ without a nuspec dependency.
```

```
Fix duplicate key crash in IdentityStorage.Populate

The upsert used InsertOne which threw on existing identities.
Replace with ReplaceOne using upsert: true.
```

**Bad examples** (never do these):

- `Fix stuff`
- `WIP`
- `Added files`
- `Fix bug and add feature and update docs`

## Step 4 — Push the branch

**Stop after step 3 for commit-only.** Run this step only for an authorized push; push-only does not authorize new commits or a PR.

```
git push -u origin <branch-name>
```

## Step 5 — Create the PR

**Stop after step 4 for push-only.** Create a PR only when requested; PR-only stops before merge.

Use `mcp_github_github_create_pull_request` with:

- `owner`: derive from the current repository's `origin` remote
- `repo`: derive from the current repository's `origin` remote
- `head`: the branch name
- `base`: `main`
- `title`: short imperative sentence describing the overall change
- `body`: PR description (see below)

### PR description format

Follow `.github/pull_request_template.md`. Include only non-empty sections.

```markdown
## Added
- <release-note bullet> (#<actual-issue-number>)

## Changed
- <release-note bullet> (#<actual-issue-number>)

## Fixed
- <release-note bullet> (#<actual-issue-number>)
```

Rules:
- Bullets are short, release-note ready, written for a user reading the changelog.
- End every bullet with `(#<N>)` using the **real** GitHub issue number. Search issues first. If there is no issue, omit the reference entirely — never write `(#issue)` or reuse an example number.
- Remove any empty sections — no blank headings.
- Include a `# Summary` paragraph only when the bullets alone don't tell the full story.
- Never include any Copilot prompt transcript or "Original prompt" block.

### Searching for a related issue

```
mcp_github_github_search_issues  query="<keywords> repo:<owner>/<repo>"
```

If nothing relevant is found, omit the issue reference from affected bullets.

## Step 6 — Confirm release intent

**Release intent** — propose `major`, `minor`, `patch`, or repository-supported non-release intent (ordinarily `no-release`) from the actual impact; confirm the current workflow contract. Apply a label only with explicit authorization for that exact label and any publication effects. If the user forbids labels but the repository requires one, report the blocker; do not silently omit it or bypass the gate.

Documentation-only changes use repository-supported non-release intent, ordinarily `no-release`; confirm the workflow contract rather than assuming a label or API state. Run relevant content, link, frontmatter, and corpus checks instead of unrelated application builds, and satisfy every repository-required check, including release-intent checks where supported. Documentation is never a blanket exemption from red CI.

Read back an authorized label/body edit to confirm the exact requested change; API errors or ambiguous results require reconciliation, not blind retries. This skill assumes no external API state.

## Step 7 — Wait for required CI

Documentation-only work is not exempt from required CI or release-intent checks.
Inspect required checks read-only with the repository-supported tools. Before any
separately authorized merge, every required check must pass. Diagnose a failure
within a bounded attempt, fix only in-scope causes with authorized additive
commits/pushes, and re-run the affected gate. Report unrelated, environmental, or
unresolved failures as blockers; do not keep editing or retrying indefinitely and
do not treat an expected failure as green.

## Step 8 — Merge the PR

**Separate explicit merge authorization required for the exact PR/head and declared effects. PR-only stops here.** Required checks must pass; use a true merge commit, never squash or rebase. A release label is not merge/publication authority.

**Never squash-merge. Preserve every commit and its history.**

Immediately before merging:

1. Verify that the PR is mergeable and all required checks pass.
2. Verify that the selected merge method is not squash.
3. Use `merge` / **Create a merge commit** only; never rewrite history with rebase.
4. If the allowed merge method is unavailable or cannot be verified, stop and do not merge. Never fall back to squash.

Use `mcp_github_github_merge_pull_request` with:

- `merge_method`: `merge` only; never `rebase` or `squash`
- `owner`: derive from the current repository's `origin` remote
- `repo`: derive from the current repository's `origin` remote
- `pullNumber`: the PR number returned in step 5

For the GitHub CLI, use `gh pr merge <number> --merge` only after exact merge authorization. Never pass `--rebase` or `--squash`.

After merging, verify that every PR commit remains represented in the resulting history. If it does not, report the problem immediately; do not rewrite history; use an additive recovery.

## Step 9 — Clean up the branch

Branch deletion is optional, not completion criteria. Only after a verified merge
and separate explicit authorization for each exact local/remote ref may cleanup
proceed through the repository’s mutation protocol. Capture pre-state and inverse
escrow, recheck refs immediately before each action, and read back each outcome.
Do not batch checkout, pull, and deletion into one unreviewed command. Use only
`git branch -d`, never `-D`; stop if it refuses or any ref/precondition drifts.
Leave branches intact and report pending cleanup when authorization is absent.

## Full example sequence

These are separate authorized endpoints, not one command batch. Paths and IDs
are placeholders; derive the current repository and real targets before acting.

```bash
# Commit-only: explicitly authorized paths, review, commit, then STOP.
git add <authorized-path>
git diff --cached
git commit -m "Fix the scoped behavior"

# Only when push was requested: push authorized commits, then STOP for push-only.
git push -u origin <authorized-branch>

# Only when PR creation was requested: use the reviewed template/body.
gh pr create --base main --head <authorized-branch> --body-file <reviewed-body-path>
# STOP before merge for PR-only. Report relevant required checks.
```

A release-intent label, merge, issue comment/closure, or branch deletion is not an
implied next command. First obtain separate explicit authorization for the exact
effect and satisfy the authorization, current workflow, and mutation gates above.

## Common mistakes to avoid

- **Never `git add .`** — always stage specific files and verify with `git diff --cached`.
- **Never invent issue numbers** — search first; omit the reference if nothing matches.
- **Never leave placeholder text** in PR bodies (`(#issue)`, `(#123)`).
- **Never commit code that does not compile** — every commit must be a working state.
- **Never push directly to `main`** — always go through the branch + PR flow.
- **Never squash-merge or rebase-merge** — use a normal merge commit only when separately authorized.
- **Never delete branches automatically** — leave local/remote refs intact unless their exact deletion is separately authorized after verified merge.
