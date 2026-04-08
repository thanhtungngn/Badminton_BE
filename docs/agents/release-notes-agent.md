# Release Notes Agent Instructions

You are a GitHub Copilot coding agent working on the **Badminton_BE** repository. This file provides instructions for automatically generating structured release notes when a `release/*` branch is merged into `master`.

## Your Role

When triggered, you will receive:
- The version number extracted from the merged release branch name (e.g., `release/v1.0.3` → `v1.0.3`).
- A list of commit messages and PR titles since the previous release tag.

Your job is to produce a well-structured Markdown release notes file and save it to `docs/releases/<version>.md`.

## Output Format

Use the following structure for the release notes file. Omit sections that have no content.

```markdown
# Release Notes - <version>

## Version
`<version>`

## Summary
<One or two sentences summarising the overall theme of this release.>

## New Features

### <Feature Name>
- Description of the feature.
- Any relevant endpoint changes.
- Behaviour details.

## Bug Fixes

### <Fix Name>
- What was broken and how it was fixed.

## Improvements

### <Improvement Name>
- What was improved.

## Breaking Changes

### <Change Name>
- What changed and what consumers need to update.

## Documentation
- List any documentation files added or updated.

## Notes
<Any additional notes, caveats, or migration instructions.>
```

## Categorisation Rules

- Commits/PRs with `feat:` or `feature:` prefix → **New Features**
- Commits/PRs with `fix:` or `bugfix:` prefix → **Bug Fixes**
- Commits/PRs with `improve:`, `refactor:`, `perf:`, or `chore:` prefix → **Improvements**
- Commits/PRs with `breaking:` or `BREAKING CHANGE` → **Breaking Changes**
- Commits/PRs with `docs:` prefix → **Documentation**
- Merge commits and version-bump commits should be excluded.

## File Location

Save the generated file to:
```
docs/releases/<version>.md
```
For example, for version `v1.0.3`, save to `docs/releases/v1.0.3.md`.

## Project Context

The Badminton_BE API is a multi-tenant badminton club management system. Key domain concepts:
- **Sessions** — badminton sessions that players join.
- **Members / Players** — participants in sessions.
- **Payments** — player payment tracking per session.
- **Matches** — in-session match records with Elo scoring.
- **Rankings** — player Elo-based ranking tiers.
- **Auth** — JWT-based authentication with per-user data isolation.

Use these domain terms in the release notes for clarity.

## Existing Release Notes Examples

Refer to `docs/releases/v1.0.1.md` and `docs/releases/v1.0.2.md` for tone and formatting examples.
