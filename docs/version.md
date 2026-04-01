# Version History

## v1.1.2 — MCP Server: Render.com Deployment (SSE/HTTP Transport)
> Branch: `copilot/suggest-deployment-options`

### Changed — Badminton_MCP
- **Transport switched from stdio → HTTP/SSE** for cloud hosting on Render.com.
  - `Program.cs` now uses `WebApplication` and `.WithHttpTransport()` / `app.MapMcp()`.
  - MCP clients connect over SSE at `GET /sse` (or the streamable HTTP endpoint `POST /mcp`).
- **Project SDK changed** from `Microsoft.NET.Sdk` (console) to `Microsoft.NET.Sdk.Web`.
- **Package swap**: replaced `ModelContextProtocol` + `Microsoft.Extensions.Hosting` with `ModelContextProtocol.AspNetCore 1.2.0`.

### Added — Infrastructure
- **`Dockerfile.mcp`** — multi-stage Docker image for the MCP project; listens on `$PORT` (Render) or `8080` (local).
- **`render.yaml`** — added `badminton-mcp` web service pointing at `Dockerfile.mcp` with all required env-var placeholders.
- **`.mcp.json.example`** — added `badminton-mcp-remote` SSE entry with the Render URL template.

### How to communicate with the deployed server
Connect any MCP client (GitHub Copilot, Claude Desktop, Cursor, etc.) using the **SSE** transport type:
- **URL:** `https://badminton-mcp.onrender.com/sse`  *(replace with your actual Render service URL)*
- In `.mcp.json`: set `"type": "sse"` and `"url": "https://<your-render-url>/sse"`.

---

## v1.1.1 — Badminton API MCP Tools
> Branch: `AI/badminton-api-mcp-tools`

### Added — Badminton_MCP
- **`Tools/BadmintonTools.cs`** — 13 new MCP tools exposing the full Badminton REST API to GitHub Copilot:

  | Tool | Endpoint |
  |------|----------|
  | `Login` | `POST /api/auth/login` — authenticates and stores JWT for the session |
  | `GetSessions` | `GET /api/session` |
  | `GetDashboardSessions` | `GET /api/session/dashboard` |
  | `GetSessionDetail` | `GET /api/session/{id}/detail` |
  | `CreateSession` | `POST /api/session` |
  | `UpdateSession` | `PUT /api/session/{id}` |
  | `GetMembers` | `GET /api/member` |
  | `GetMemberById` | `GET /api/member/{id}` |
  | `LookupMemberByContact` | `GET /api/member/lookup?contactValue=...` |
  | `AddMemberToSession` | `POST /api/sessionplayer` |
  | `RemoveMemberFromSession` | `DELETE /api/sessionplayer/{id}` |
  | `UpdateSessionPlayerStatus` | `PATCH /api/sessionplayer/{id}/status` |
  | `SetSessionPricing` | `POST /api/payment/session/{id}` |
  | `PaySessionPlayer` | `POST /api/payment/session-player/{id}/pay` |

- **`BadmintonApiClient`** extended:
  - `_token` field reads `BADMINTON_API_TOKEN` env var at startup.
  - `SetToken(string)` method stores the JWT after a `Login` call.
  - `PutAsync`, `PatchAsync`, `DeleteAsync` methods added.
  - Token applied automatically to all requests.

---


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
