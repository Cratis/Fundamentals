# Shared AI Assistant Configuration

This folder maintains legacy repository-local AI assistant artifacts; it is not a shared distribution package or propagation hub.

## Distribution and local adapters

Cross-repository broadcast, all-to-all propagation, and reverse synchronization
are retired. Do not run legacy propagation or turn a consuming repository into a
hub. Shared public-safe behavior is authored and reviewed in `Cratis/AI`, generated
into `Cratis/AI.Distribution`, and consumed only at an immutable reviewed version
after release gates pass. Propose sanitized reusable improvements upstream for
review; never reverse-sync private trees or local facts.

These legacy repository-local rules remain locally maintained during canary;
this is not permission to patch generated immutable distribution bytes or copy
whole AI trees. Preserve private/project overlays, local skills, and minimal
host bootstraps. Keep legacy adapters and actual workflows in place until an
approved replacement passes canary and reviewed retirement gates. Update shared
packages via approved exact-version pins; roll back by version.


## Structure

- `rules/` contains shared instruction files.
- `prompts/` contains reusable prompt templates.
- `agents/` contains reusable agent definitions.

## Tool integration

- GitHub Copilot files under `.github/` are symlinks to files in `.ai/`.
- Claude Code files under `.claude/` are symlinks to files in `.ai/`.

## Scoped rule frontmatter

Scoped rules include both:

- `applyTo` for GitHub Copilot instruction matching.
- `paths` for Claude Code rule matching.

When adding or changing a shared rule, update the file in `.ai/rules/` only. See `rules/managing-ai-rules.md` for the full guide on adding, updating, and renaming rules.
