# Badminton_BE Current State

## Overview
`Badminton_BE` is an ASP.NET Core Web API for managing badminton sessions, members, rankings, payments, and user authentication.

## Tech Stack
- `.NET 10`
- `ASP.NET Core Web API`
- `Entity Framework Core`
- `Pomelo.EntityFrameworkCore.MySql`
- `JWT` authentication
- `Swagger / Swashbuckle`

## Main Functional Areas

### 1. Authentication and User Profile
Handled by `AuthController` and `AuthService`.

Current capabilities:
- Register account
- Login and receive JWT
- Logout by revoking token
- Get current user profile
- Update current user profile

Main endpoints:
- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/logout`
- `GET /api/auth/profile`
- `PUT /api/auth/profile`

## 2. Member Management
Handled by `MemberController`, `MemberService`, and `MemberRepository`.

Current capabilities:
- Create member with contacts
- Get all members
- Get member by id
- Get member by contact value
- Update member and replace contacts
- Delete member
- Anonymous lookup by contact to retrieve player/session information

Main endpoints:
- `POST /api/member`
- `GET /api/member`
- `GET /api/member/{id}`
- `GET /api/member/by-contact?contactValue=...`
- `GET /api/member/lookup?contactValue=...`
- `PUT /api/member/{id}`
- `DELETE /api/member/{id}`

Lookup response currently includes:
- member id
- name
- contact value
- level
- elo point
- ranking name
- joined sessions
- payment status per session

## 3. Session Management
Handled by `SessionController` and `SessionService`.

Current capabilities:
- Create session
- Get all sessions
- Get active sessions for dashboard
- Get session by id
- Get detailed session info with players
- Update session
- Delete session

Main endpoints:
- `POST /api/session`
- `GET /api/session`
- `GET /api/session/dashboard`
- `GET /api/session/{id}`
- `GET /api/session/{id}/detail`
- `PUT /api/session/{id}`
- `DELETE /api/session/{id}`

Session detail currently includes player data such as:
- member id
- name
- contact
- level
- elo point
- paid status
- calculated price

## 4. Session Player Management
Handled by `SessionPlayerController` and `SessionPlayerService`.

Current capabilities:
- Add member to session
- Get session-player record by id
- Update session-player status
- Remove member from session
- Prevent duplicate member in same session
- Prevent overlapping upcoming/ongoing sessions for the same member

Main endpoints:
- `POST /api/sessionplayer`
- `GET /api/sessionplayer/{id}`
- `PATCH /api/sessionplayer/{id}/status`
- `DELETE /api/sessionplayer/{id}`

Session-player response currently includes:
- session id
- member id
- level
- elo point
- status
- timestamps

## 5. Payment Management
Handled by `PaymentController` and `PaymentService`.

Current capabilities:
- Set session pricing for male/female players
- Pay by `sessionPlayerId`
- Auto-create player payments when needed

Main endpoints:
- `POST /api/payment/session/{sessionId}`
- `POST /api/payment/session-player/{sessionPlayerId}/pay`

### Current payment behavior
Player payments are created automatically when:
- a session becomes `OnGoing`
- a member is added after the session has already started
- session prices are set for an ongoing session

There is no longer a public endpoint for manual bulk payment generation.

## 6. Ranking System
Handled by ranking repositories and services.

Current capabilities:
- store ranking tiers
- maintain player ranking per member
- expose ranking information in member lookup and player/session APIs

Ranking-related data currently includes:
- ranking name
- elo point
- wins / losses / draws
- matches played

## Core Domain Models
Main entities in the application:
- `AppUser`
- `Member`
- `Contact`
- `Session`
- `SessionPlayer`
- `SessionPayment`
- `PlayerPayment`
- `Ranking`
- `PlayerRanking`
- `RevokedToken`

## Multi-Tenant Behavior
The application uses `UserId`-based ownership filtering for tenant isolation.

Current rule in the codebase:
- domain tables are filtered by authenticated user
- `PlayerRanking` is intentionally not tenant-filtered
- anonymous lookup paths explicitly rely on bypassing tenant restrictions where required

## Notable Implementation Details
- EF Core global query filters are used for user-owned entities.
- `CurrentUserService` reads the current user id from claims.
- `AppDbContext` automatically sets timestamps.
- `AppDbContext` also assigns `UserId` on insert for user-owned entities when a current user exists.
- Swagger metadata is already present on controllers.
- JWT revocation is supported through `RevokedToken`.

## Suggested Next Documentation Files
If needed, this `docs` folder can be expanded with:
- `docs/api-endpoints.md`
- `docs/domain-model.md`
- `docs/payment-flow.md`
- `docs/ranking-flow.md`
- `docs/deployment.md`
