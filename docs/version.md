# Version History

## Unreleased

### Fixed
- **Bug - API get members doesn't return stats property** (Trello: https://trello.com/c/9Hj3qZoR)
  - `GetMembersAsync()` in `MemberService` was calling `MapToReadDto()` directly, leaving `Wins`, `Losses`, `Draws`, and `WinRate` as 0 for every member in the list response.
  - Fixed by iterating members and calling `BuildMatchStatsAsync()` for each — consistent with `GetMemberByIdAsync()`.
  - Trello card moved to **Code Review and Testing**.

- **Bug - User still can register while there's no slot left** (Trello: https://trello.com/c/HITDNrCY)
  - Added `CountActiveBySessionAsync(int sessionId)` to `ISessionPlayerRepository` and `SessionPlayerRepository` — counts players with any status except `Canceled`.
  - Added capacity guard in `SessionPlayerService.AddMemberToSessionAsync()`: when `MaxPlayerPerCourt` is set, rejects registration if active player count ≥ `NumberOfCourts × MaxPlayerPerCourt`.
  - Trello card moved to **Code Review and Testing**.

### Added
- `.mcp.json` — local-only MCP client config (gitignored) for wiring the MCP server to GitHub Copilot in Visual Studio 2026.
- `.mcp.json.example` — committed template showing the required server config and env var names.
- `trello.runsettings` — local-only run settings file (gitignored) for supplying Trello credentials when debugging/running integration tests in Visual Studio.
- `trello.runsettings.example` — committed template showing required env vars and how to obtain each value.
- `Badminton_MCP.Tests` project — xUnit integration test project targeting .NET 10, added to `Badminton_BE.slnx`.
- `TrelloConnectionTests` — 4 integration tests covering `GetBoardLists`, `GetBoardCards`, `GetMemberCards`, and `GetBoardAICards`. All tests skip gracefully when `TRELLO_API_KEY`/`TRELLO_TOKEN` env vars are not set.
- `docs/mcp-workflow.md` — MCP server workflow document covering stdio transport, Trello board structure, tool call sequences, a full session example, and a setup checklist.
- `docs/mcp-server.md` — full documentation for the `Badminton_MCP` MCP server project covering architecture, tools, environment variables, client configuration, and next steps.
- `docs/version.md` — this version history file.

---

## Previous Changes

### Badminton_MCP project
- Initial MCP server created with stdio transport.
- `TrelloClient` implemented with support for member cards, board cards, board lists, move card, and add comment.
- `TrelloTools` registered with five MCP tools: `GetMyAICards`, `GetBoardAICards`, `GetBoardLists`, `MoveCardToList`, `AddCommentToCard`.
- `BadmintonApiClient` created with `GetAsync` and `PostAsync` methods (no tools yet).

### Badminton_BE project
- Authentication: register, login, logout, profile.
- Member management: CRUD, contact lookup, anonymous lookup.
- Session management: create, read, update, delete, dashboard, detail with players and matches.
- Match management: full CRUD with team/score/winner validation.
- Session player management: join/leave, status update, duplicate and overlap prevention.
- Payment management: set session pricing, pay by session player, auto-create payments on session state change.
- Ranking system: ranking tiers, player ranking per member.
- Multi-tenant isolation via `UserId` on all domain tables (except `PlayerRanking`).
- JWT authentication with token revocation support.
