# Jira Dev Agent Instructions

You are a GitHub Copilot coding agent working on the **Badminton_BE** repository. This file provides instructions for handling tasks sourced from Jira tickets tagged with the **AI** label.

## Your Role

When you are assigned a GitHub Issue created from a Jira ticket, your job is to:

1. Read the issue title and description — the body contains the original Jira ticket content.
2. Identify the Jira ticket key (format: `[PROJ-123]` in the title and `jira-issue-key:PROJ-123` in the body).
3. Implement the change following all project conventions below.
4. Open a Pull Request targeting `master` with a clear title and description.

## Project Conventions

- **Language & framework:** C# (.NET 10), ASP.NET Core Web API.
- **Multi-tenancy:** Every domain table must include a `UserId` column. All data queries must filter by the authenticated user's ID. Exception: `PlayerRanking` — no `UserId` filtering.
- **DTOs and Models:** Never define DTO or Model classes inside service classes. Always place them in their own file under `DTOs/` or `Models/`.
- **Version tracking:** Every code change must be documented in `docs/version.md` and `docs/current-state.md` (if architecture changed).
- **Version bump:** Increment the patch digit in both `.csproj` files **once per ticket/branch** — at the start of the branch, not on every commit. All commits on the same branch share the same version number. Create a release document under `docs/releases/vX.Y.Z.md` when the version is bumped.
- **Naming:** PascalCase for C# types and members. camelCase in JSON responses.
- **Architecture:** Repository → Service → Controller layered pattern.
- **Database:** EF Core with MySQL (Pomelo). Run `dotnet ef migrations add <Name>` whenever the schema changes.
- **Build:** Must pass `dotnet build` with no errors before opening a PR.

## Repository Structure

```
Badminton_BE/           ← main API project
  Controllers/          ← ASP.NET Core controllers
  Services/             ← business logic
  Repositories/         ← EF Core data access
  DTOs/                 ← request / response DTOs
  Models/               ← EF Core entity models
  Data/                 ← DbContext
  Migrations/           ← EF Core migrations
docs/
  version.md            ← changelog (update every change)
  current-state.md      ← architecture snapshot (update if structure changes)
Badminton_MCP/          ← MCP server (tool extensions)
  Tools/                ← MCP tool classes
```

## Workflow

1. **Branch** — create a branch from `master` using the Jira ticket type and key:
   - Format: `feature/<jira-key>` for new features, `bug/<jira-key>` for bug fixes.
   - Example: feature ticket `SB-42` → `feature/SB-42`; bug ticket `SB-99` → `bug/SB-99`
   - Determine type from the Jira issue type field (Story / Feature → `feature/`, Bug → `bug/`).
2. **Implement** — make only the changes needed to fulfil the ticket.
3. **Document** — update `docs/version.md`; update `docs/current-state.md` if the API surface or data model changed; create `docs/releases/vX.Y.Z.md` for the new version.
4. **Bump version** — increment patch in `Badminton_BE/Badminton_BE.csproj` and `Badminton_MCP/Badminton_MCP.csproj` **once when the branch is created**. Do not bump again on subsequent commits to the same branch.
5. **Build** — run `dotnet build` and fix any errors.
6. **PR** — open a PR to `master`.
   - Title format: `feat|fix|chore: <summary> (vX.Y.Z)`
   - Body: summary of changes, closes the GitHub issue (`Closes #<number>`), links to Jira ticket.
7. **Jira comment** — after the PR is created, post a comment on the Jira ticket using `AddJiraComment`:
   - Include a short summary of what was implemented.
   - Include the PR URL for review.
   - Example comment format:
     ```
     Dev work completed.

     Summary: <what was implemented>

     PR for review: <PR URL>
     ```
8. **Move ticket** — transition the Jira ticket to **Code Review** using `TransitionJiraIssue(key: "PROJ-123", transitionName: "Code Review")`.

## PR Requirements

- Title matches the Jira ticket summary (prefixed with fix/feat/chore).
- Body references both the GitHub issue and the Jira ticket URL.
- Include migration files if schema changed.
- `docs/version.md` must be updated.

## Jira Ticket Identifier

The originating Jira ticket key is embedded in the issue body as:
```
**jira-issue-key:PROJ-123**
```
Use this to avoid duplicate work if the same ticket appears again.
