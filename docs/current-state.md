# Badminton Project — Current State (v1.1.1)

## Solution Overview

The solution (`Badminton_BE.slnx`) contains three projects:

| Project | Type | Version | Purpose |
|---------|------|---------|---------|
| `Badminton_BE` | ASP.NET Core Web API | 1.1.0 | Core backend — sessions, members, payments, rankings |
| `Badminton_MCP` | .NET 10 Console (MCP Server) | 1.1.0 | AI tooling — exposes Trello and Badminton API tools to GitHub Copilot |
| `Badminton_MCP.Tests` | xUnit Test Project | — | Integration tests for the MCP server's Trello connection |

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

## Badminton_MCP — AI Tooling Server

### Tech Stack
- `.NET 10` console application
- `ModelContextProtocol` 1.2.0 — MCP server framework
- `Microsoft.Extensions.Hosting` — DI and generic host
- `Microsoft.Extensions.Http` — `IHttpClientFactory`

### Transport
JSON-RPC 2.0 over **stdio**. The server is spawned once per VS session by GitHub Copilot (via `.mcp.json`) and stays alive for the duration of the session.

### Registered Tools — Trello

| Tool | Description |
|------|-------------|
| `GetMyAICards` | Open AI-labelled cards assigned to `TRELLO_MEMBER_ID` |
| `GetBoardAICards` | All AI-labelled cards on the board, optionally filtered by member |
| `GetBoardLists` | All lists on the board (used to resolve list IDs) |
| `MoveCardToList` | Moves a card to a specified list |
| `AddCommentToCard` | Posts a comment on a card (e.g. PR links) |

### Registered Tools — Badminton API

| Tool | Description |
|------|-------------|
| `Login` | Authenticates with username/password, stores JWT for the session |
| `GetSessions` | All sessions for the authenticated user |
| `GetDashboardSessions` | Active (Upcoming/OnGoing) sessions only |
| `GetSessionDetail` | Full session detail — players, payments, matches |
| `CreateSession` | Create a new session with pricing |
| `UpdateSession` | Update session fields or change status |
| `GetMembers` | All members with ranking and stats |
| `GetMemberById` | Single member full profile |
| `LookupMemberByContact` | Anonymous lookup by phone/email/Facebook |
| `AddMemberToSession` | Add a player to a session |
| `RemoveMemberFromSession` | Remove a player from a session |
| `UpdateSessionPlayerStatus` | Set player status (Joined/Canceled/Paid/NotPaid) |
| `SetSessionPricing` | Set male/female pricing for a session |
| `PaySessionPlayer` | Record a payment from a player |

### Environment Variables

| Variable | Used by |
|----------|---------|
| `TRELLO_API_KEY` | All Trello tools |
| `TRELLO_TOKEN` | All Trello tools |
| `TRELLO_BOARD_ID` | `GetBoardAICards`, `GetBoardLists`, `MoveCardToList` |
| `TRELLO_MEMBER_ID` | `GetMyAICards` |
| `BADMINTON_API_URL` | `BadmintonApiClient` (tools planned for v1.2) |

### Planned for v1.2
`BadmintonApiClient` is wired into DI but has no tools yet. Planned tools: `GetSessions`, `GetSessionDetail`, `GetMembers`, `GetMemberById`, `GetSessionMatches`, `SetSessionPricing`, `PaySessionPlayer`.

---

## Developer Tooling

| File | Purpose |
|------|---------|
| `.mcp.json` | Local MCP config for GitHub Copilot in VS (gitignored) |
| `.mcp.json.example` | Committed template |
| `trello.runsettings` | Local test credentials for VS Test Explorer (gitignored) |
| `trello.runsettings.example` | Committed template |
| `docs/mcp-server.md` | MCP server reference documentation |
| `docs/mcp-workflow.md` | End-to-end AI workflow guide |
| `docs/version.md` | Version history |
| `docs/new-user-guide.md` | Guide for new club owners using the API |
