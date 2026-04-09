# Trello AI Agent Instructions

You are a GitHub Copilot coding agent working on the **Badminton_BE** repository. This file provides instructions for handling tasks sourced from Trello cards tagged with the **AI** label and assigned to the project owner.

## Your Role

When you are assigned a GitHub Issue that was created from a Trello card, your job is to:

1. Read the issue title and description carefully — the description contains the original Trello card content.
2. Understand what needs to be implemented, fixed, or improved.
3. Implement the change following all project conventions below.
4. Open a Pull Request targeting the `master` branch with a clear title and description.

## Project Conventions

- **Language & framework:** C# (.NET 10), ASP.NET Core Web API.
- **Multi-tenancy:** Every domain table must include a `UserId` column. All data queries must filter by the authenticated user's ID. Exception: `PlayerRanking` — no `UserId` filtering.
- **DTOs and Models:** Never define DTO or Model classes inside service classes. Always place them in their own file under `DTOs/` or `Models/`.
- **Version tracking:** Every code change must be documented in three places:
    - Add a brief entry to `docs/version.md` (changelog).
    - Create `docs/releases/vX.Y.Z.md` with the full release notes for the new patch version.
    - Update `docs/current-state.md` if any capability, endpoint, model, or project structure changed.
- **Naming:** Use PascalCase for C# types and members. Use camelCase in JSON responses.
- **Architecture:** Follow the existing Repository → Service → Controller layered pattern.
- **Database:** Use EF Core with MySQL (Pomelo). Run `dotnet ef migrations add <Name>` when schema changes are needed.

## Repository Structure

```
Badminton_BE/           ← main API project
  Controllers/          ← ASP.NET Core controllers
  Services/             ← business logic
  Repositories/         ← EF Core repositories
  DTOs/                 ← request/response DTOs
  Models/               ← EF Core entity models
  Data/                 ← DbContext
  Migrations/           ← EF Core migrations
docs/releases/          ← release notes per version
Badminton_MCP/          ← MCP server (tool extensions)
  Tools/                ← MCP tool classes
```

## PR Requirements

- Title: match the Trello card title.
- Description: summarise what was changed and reference the original issue with `Closes #<issue-number>`.
- Include migration files if schema was changed.
- Add a brief entry to `docs/version.md` and create `docs/releases/vX.Y.Z.md` with full release notes.
- Update `docs/current-state.md` if any capability, endpoint, model, or project structure changed.

## Trello Card Identifier

The originating Trello card ID is embedded in the issue body as:
```
**Trello Card ID:** trello-card-id:<id>
```
Use this to avoid duplicate work if the same card appears again.
