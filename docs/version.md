# Version History

## v1.1.0 — MCP Server & Bug Fixes
> Branch: `release/v1.1.0`

### Added — Badminton_MCP Server
- `Badminton_MCP` project: .NET 10 MCP server exposing Trello and Badminton API tools to GitHub Copilot via JSON-RPC over stdio.
- `TrelloClient` — Trello REST API v1 wrapper (member cards, board cards, board lists, move card, add comment).
- `TrelloTools` — five MCP tools registered with `[McpServerTool]`:
  - `GetMyAICards` — returns open AI-labelled cards assigned to the configured member.
  - `GetBoardAICards` — returns all AI-labelled cards on the board, optionally filtered by member.
  - `GetBoardLists` — returns all lists on the board (used to resolve list IDs before moving cards).
  - `MoveCardToList` — moves a card to a target list.
  - `AddCommentToCard` — posts a comment on a card (e.g. PR links).
- `BadmintonApiClient` — generic HTTP GET/POST wrapper for the Badminton REST API; wired into DI, tools planned for v1.2.
- `.mcp.json` — local-only MCP client config (gitignored) for GitHub Copilot in Visual Studio.
- `.mcp.json.example` — committed template with required server config and env var names.

### Added — Tests & Developer Tooling
- `Badminton_MCP.Tests` — xUnit integration test project targeting .NET 10, added to `Badminton_BE.slnx`.
- `TrelloConnectionTests` — 4 integration tests (`GetBoardLists`, `GetBoardCards`, `GetMemberCards`, `GetBoardAICards`). All skip gracefully when Trello env vars are not set.
- `trello.runsettings` — local-only run settings file (gitignored) for supplying Trello credentials in Visual Studio Test Explorer.
- `trello.runsettings.example` — committed template with required env vars and instructions for obtaining each value.

### Added — Documentation
- `docs/version.md` — this version history file.
- `docs/mcp-server.md` — architecture, tools, environment variables, client configuration, and next steps for the MCP server.
- `docs/mcp-workflow.md` — stdio transport diagram, Trello board structure, per-tool call flows, a full session example, and a setup checklist.

### Fixed — Badminton_BE
- **Member list response missing stats** (`GET /api/member`)
  - `GetMembersAsync()` returned `Wins`, `Losses`, `Draws`, and `WinRate` as `0` for every member because it never called `BuildMatchStatsAsync()`.
  - Fixed by iterating members and calling `BuildMatchStatsAsync()` for each — consistent with `GetMemberByIdAsync()`.

- **Session registration not enforcing capacity**
  - `AddMemberToSessionAsync()` had no slot check — players could join a full session.
  - Added `CountActiveBySessionAsync(int sessionId)` to `ISessionPlayerRepository` / `SessionPlayerRepository` (counts non-`Canceled` players).
  - Added capacity guard: blocks registration when `activeCount >= NumberOfCourts × MaxPlayerPerCourt`. Only enforced when `MaxPlayerPerCourt` is explicitly set on the session.

---

## v1.0.2 — Hotfix
- Return `PlayerPaymentId` in payment response.
- Add update payment amount API.

## v1.0.1 — Core Backend
- Authentication: register, login, logout, profile, JWT revocation.
- Member management: CRUD, contact lookup, anonymous lookup.
- Session management: create, read, update, delete, dashboard, detail with players and matches.
- Match management: full CRUD with team/score/winner validation.
- Session player management: join/leave, status update, duplicate and overlap prevention.
- Payment management: set session pricing, pay by session player, auto-create player payments.
- Ranking system: ranking tiers, ELO points, player ranking per member.
- Match stats: wins, losses, draws, win rate per member.
- Multi-tenant isolation via `UserId` on all domain tables (except `PlayerRanking`).
- Public API for anonymous session registration and member lookup.
