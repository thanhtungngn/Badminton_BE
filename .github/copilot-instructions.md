# Copilot Instructions

## Project Guidelines
- The user prefers multi-tenant data isolation: all domain tables should include a UserId so each authenticated user can only access their own data, except for PlayerRanking which should not have UserId filtering.
- Whenever code is updated in this repository, the change should be noted in the version document, not just general project documentation.
- Do not define model or DTO classes inside service classes. Always place them in their own file in the DTOs or Models folder.
- All new controllers, services, and repositories **must** have corresponding unit tests in `Badminton_BE.Tests` before the PR is merged. Tests must maintain ≥80% line coverage across the project. Follow the existing patterns: use `Moq` for mocks, `InMemory` DB via `DbContextFactory` for repository/service tests, and mock interfaces directly for controller tests.

## Terminology
- **User** — a player who does **not** need authentication. Can access public-facing content (e.g. session registration, member lookup).
- **Owner** — a player who **requires** authentication. Manages sessions, members, payments, and notifications.

## Subtask Rules
When creating subtasks (e.g. from a Trello card or implementation plan):
- Subtasks **inherit** the parent task's branch, labels, and workflow context — do not create a new branch per subtask.
- **No duplicate subtasks** — check for existing equivalent subtasks before adding. 
- Each subtask must have a distinct, specific scope; if two subtasks overlap, merge them into one.


When working on a Trello card tagged with the **AI** label, always follow this sequence:

1. **Branch** — all work must be done on a branch named `AI/<card-slug>`.
   - Slug = card name lowercased, spaces → hyphens, special chars stripped.
   - Branch from `master`. Use `scripts/ai-start.ps1 -CardName "..."` to automate this.

2. **Implement** — make the code changes. Build must pass before proceeding.

3. **Document** — before committing:
   - Add an entry to `docs/version.md` describing what changed.
   - Update `docs/current-state.md` if any capability, endpoint, model, or project structure changed.

4. **Bump patch version** — increment the last digit of `<Version>` in both `.csproj` files **once when the branch is created**, not on every commit. All commits on the same branch share the same version.
   - Create `docs/releases/vX.Y.Z.md` describing what changed.
   - Register the release doc in `Badminton_BE/Badminton_BE.csproj` as a `<None Include>` link **before pushing**.
   - Example: `1.1.0` → `1.1.1`

5. **Commit and push** — single commit: `fix|feat|chore: <summary> (vX.Y.Z)`

6. **Pull Request** — open a PR from `AI/<slug>` → `master` using `gh pr create`.
   - PR title: `fix|feat|chore: <summary> (vX.Y.Z)`
   - PR body: summary, Trello card URL, version, link to `docs/version.md`.

7. **Trello** — after PR is created:
   - Post a comment on the card with the PR URL and a summary of changes.
   - Move the card to the **Code Review and Testing** list (`69b053ecd1ed8f9bcd01e51e`).
   - Use `scripts/ai-finish.ps1 -CardId <id> -Summary "..."` to automate steps 5–7.

Full workflow details: `docs/ai-task-workflow.md`
