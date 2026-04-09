# Badminton Project — Current State (v1.1.7)

## Solution Overview

The solution (`Badminton_BE.slnx`) contains three projects:

| Project | Type | Version | Purpose |
|---------|------|---------|---------|
| `Badminton_BE` | ASP.NET Core Web API | 1.1.2 | Core backend — sessions, members, payments, rankings |
| `Badminton_MCP` | ASP.NET Core Web (legacy/local MCP server) | 1.1.2 | Legacy local MCP implementation; not used in the current AI workflow |
| `Badminton_MCP.Tests` | xUnit Test Project | — | Legacy tests for the local MCP server |

---

## Badminton_BE — API Backend

### Tech Stack
- `.NET 10`
- `ASP.NET Core Web API`
- `Entity Framework Core` with `Pomelo.EntityFrameworkCore.MySql`
- `JWT` authentication with token revocation
- `Swagger / Swashbuckle` with XML doc comments

### 1. Authentication and User Profile
Handled by `AuthController` and `AuthService`.

Capabilities:
- Register account
- Login and receive JWT
- Logout by revoking token
- Get current user profile
- Update current user profile

Endpoints:
- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/logout`
- `GET /api/auth/profile`
- `PUT /api/auth/profile`

### 2. Member Management
Handled by `MemberController`, `MemberService`, and `MemberRepository`.

Capabilities:
- Create member with contacts
- Get all members — response includes `Wins`, `Losses`, `Draws`, `WinRate` per member *(fixed in v1.1.0)*
- Get member by id — includes full match stats and unpaid session debt per owning user
- Get member by contact value
- Update member and replace contacts
- Delete member
- Anonymous lookup by contact — returns sessions, payment status, ranking

Endpoints:
- `POST /api/member`
- `GET /api/member`
- `GET /api/member/{id}`
- `GET /api/member/by-contact?contactValue=...`
- `GET /api/member/lookup?contactValue=...`
- `PUT /api/member/{id}`
- `DELETE /api/member/{id}`

`GET /api/member` response includes per member:
- id, name, gender, level, joinDate, avatar
- contacts
- eloPoint, rankingName
- wins, losses, draws, winRate
- unpaidByUser (debt grouped by session owner)

### 3. Session Management
Handled by `SessionController` and `SessionService`.

Capabilities:
- Create session
- Get all sessions
- Get active sessions for dashboard
- Get session by id
- Get detailed session info with players
- Get detailed session info with matches
- Update session
- Delete session

Endpoints:
- `POST /api/session`
- `GET /api/session`
- `GET /api/session/dashboard`
- `GET /api/session/{id}`
- `GET /api/session/{id}/detail`
- `PUT /api/session/{id}`
- `DELETE /api/session/{id}`

Session detail includes player data:
- member id, name, contact, level, elo point, paid status, calculated price

Session detail includes match data:
- team A and B players, scores, winner

### 4. Match Management
Handled by `SessionMatchController`, `SessionMatchService`, and `SessionMatchRepository`.

Capabilities:
- Create a match inside a session
- Get all matches of a session
- Get a single match by id
- Update match teams, score, and winner
- Delete a match
- Validate team sizes (1–2 players per team)
- Validate all players belong to the same session
- Validate winner is consistent with score when provided

Endpoints:
- `GET /api/session/{sessionId}/matches`
- `GET /api/session/{sessionId}/matches/{matchId}`
- `POST /api/session/{sessionId}/matches`
- `PUT /api/session/{sessionId}/matches/{matchId}`
- `DELETE /api/session/{sessionId}/matches/{matchId}`

### 5. Session Player Management
Handled by `SessionPlayerController` and `SessionPlayerService`.

Capabilities:
- Add member to session
- Get session-player record by id
- Update session-player status
- Remove member from session
- Prevent duplicate member in same session
- Prevent overlapping upcoming/ongoing sessions for the same member
- Enforce session capacity: blocks join when `activeCount >= NumberOfCourts × MaxPlayerPerCourt` *(added in v1.1.0)*

Endpoints:
- `POST /api/sessionplayer`
- `GET /api/sessionplayer/{id}`
- `PATCH /api/sessionplayer/{id}/status`
- `DELETE /api/sessionplayer/{id}`

`SessionPlayerStatus` values: `Joined`, `Canceled`, `Paid`, `NotPaid`

### 6. Payment Management
Handled by `PaymentController` and `PaymentService`.

Capabilities:
- Set session pricing for male/female players
- Pay by `sessionPlayerId`
- Update payment amount
- Auto-create player payments when needed

Endpoints:
- `POST /api/payment/session/{sessionId}`
- `POST /api/payment/session-player/{sessionPlayerId}/pay`

Payment auto-creation triggers:
- session transitions to `OnGoing`
- member added after session is already `OnGoing`
- session prices set while session is `OnGoing`

### 7. Ranking System
Handled by ranking repositories and `PlayerRankingService`.

Capabilities:
- Store ranking tiers (`Ranking` table)
- Maintain player ranking per member (`PlayerRanking` table)
- Expose in member list, member detail, member lookup, and session player APIs

`PlayerRanking` fields: `EloPoint`, `MatchesPlayed`, `Wins`, `Losses`, `Draws`

`PlayerRanking` is intentionally not tenant-filtered (shared across all users).

### Core Domain Models

| Model | Description |
|-------|-------------|
| `AppUser` | Authenticated user (tenant owner) |
| `Member` | Badminton player |
| `Contact` | Player contact info (phone, email, Facebook) |
| `Session` | A badminton session with time, courts, capacity |
| `SessionPlayer` | Player ↔ Session membership with payment status |
| `SessionMatch` | A match within a session |
| `SessionMatchPlayer` | Player ↔ Match assignment with team |
| `SessionPayment` | Pricing config for a session (male/female price) |
| `PlayerPayment` | Individual payment record per player per session |
| `Ranking` | Ranking tier definition (e.g. Bronze, Silver, Gold) |
| `PlayerRanking` | ELO and stats per member |
| `RevokedToken` | JWT revocation store |

### Multi-Tenant Behavior
- All domain tables (except `PlayerRanking`) carry a `UserId` column.
- EF Core global query filters enforce per-user data isolation automatically.
- `CurrentUserService` reads the authenticated user ID from JWT claims.
- `AppDbContext` assigns `UserId` on insert and sets timestamps on save.
- Anonymous lookup paths bypass filters where required.

---

## Remote MCP Server — AI Tooling Integration

### Current Runtime
- Remote deployed MCP server endpoint: `https://project-management-mcp.onrender.com/mcp`
- Workspace MCP configuration file: `.vscode/mcp.json`
- Transport used by workspace: HTTP

### Scope
The remote MCP server is used for project-management workflows and exposes Jira/Trello/GitHub tools for Copilot.

### Available Tool Domains

| Domain | Examples |
|------|-------------|
| Jira | project listing, issue search/detail, comments, transitions, issue creation |
| Trello | boards/lists/cards read and write operations |
| GitHub | repositories, branches, commits, and issue operations |

### Notes
- The local `Badminton_MCP` project remains in the repository for historical reference, but current team workflow uses the external MCP service.

---

## Developer Tooling

| File | Purpose |
|------|---------|
| `.vscode/mcp.json` | Workspace MCP config used by VS Code/Copilot |
| `.mcp.json.example` | Example MCP config (update to match active remote setup) |
| `trello.runsettings` | Legacy local test credentials for VS Test Explorer (optional) |
| `trello.runsettings.example` | Legacy template for local MCP test setup |
| `docs/mcp-server.md` | MCP server reference documentation |
| `docs/mcp-workflow.md` | End-to-end AI workflow guide |
| `docs/version.md` | Version history |
| `docs/new-user-guide.md` | Guide for new club owners using the API |
