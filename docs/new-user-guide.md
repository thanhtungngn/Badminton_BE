# New User Guide

## Purpose
This guide explains how to use `Badminton_BE` as a club owner or organizer.

## Before You Start
You need:
- a registered account
- a valid JWT access token after login
- the API base URL

Most APIs require authentication. Include your token in the `Authorization` header:

`Authorization: Bearer <your-jwt-token>`

## Main Workflow

### Step 1: Register an account
Create your account.

Endpoint:
- `POST /api/auth/register`

After registration, log in to get your JWT token.

### Step 2: Log in
Sign in with your username and password.

Endpoint:
- `POST /api/auth/login`

Result:
- JWT access token
- authenticated identity for protected APIs

### Step 3: Update your profile
Optional but recommended.

Endpoints:
- `GET /api/auth/profile`
- `PUT /api/auth/profile`

Use this to maintain your display name, avatar, phone number, bank info, and related profile fields.

### Step 4: Create members
Members are the players in your badminton group.

Endpoint:
- `POST /api/member`

A member can include:
- name
- gender
- level
- join date
- avatar
- contacts such as phone, email, or Facebook

Tips:
- keep contact values clean and consistent
- set one contact as primary when possible

### Step 5: Review or update members
Use member APIs to manage your player list.

Endpoints:
- `GET /api/member`
- `GET /api/member/{id}`
- `PUT /api/member/{id}`
- `DELETE /api/member/{id}`
- `GET /api/member/by-contact?contactValue=...`

## Session Workflow

### Step 6: Create a session
Create a badminton session before players join.

Endpoint:
- `POST /api/session`

Typical session data:
- title
- description
- start time
- end time
- address
- status
- number of courts
- max players per court

Default workflow recommendation:
- create session as `Upcoming`
- add players
- set prices
- switch to `OnGoing` when the session starts

### Step 7: Set session prices
Set pricing for male and female players.

Endpoint:
- `POST /api/payment/session/{sessionId}`

This price configuration is used to create player payment records.

### Step 8: Add players to the session
Add existing members into a session.

Endpoint:
- `POST /api/sessionplayer`

The system already prevents:
- duplicate player in the same session
- overlapping active sessions for the same member

### Step 9: Start the session
When the session starts, update it to `OnGoing`.

Endpoint:
- `PUT /api/session/{id}`

Important:
- when a session becomes `OnGoing`, player payments are generated automatically
- if a player is added after the session has already started, that player payment is also generated automatically
- if prices are set after a session is already `OnGoing`, missing payments are generated automatically

## Payment Workflow

### Step 10: Track player payments
Once a player has a `SessionPlayer` record, payment can be applied using that id.

Endpoint:
- `POST /api/payment/session-player/{sessionPlayerId}/pay`

Behavior:
- payment is created automatically if needed
- partial payment is supported
- full payment marks the payment as paid

### Step 11: Review session detail
Use the session detail endpoint to review players in a session.

Endpoint:
- `GET /api/session/{id}/detail`

Current player detail includes:
- member id
- name
- contact
- level
- elo point
- paid status
- calculated price

## Player Lookup Workflow

### Step 12: Look up a player by contact
This flow is designed for quick player lookup using contact information.

Endpoint:
- `GET /api/member/lookup?contactValue=...`

This endpoint returns:
- member name
- contact value
- level
- elo point
- ranking name
- joined sessions
- payment status

This is useful for reception, check-in, or self-service player lookup scenarios.

## Recommended Daily Operating Flow
A practical daily flow for organizers is:

1. log in
2. create or update members
3. create a session
4. set session prices
5. add players to the session
6. change session status to `OnGoing` when it starts
7. collect payments using `sessionPlayerId`
8. review session detail and player payment status

## Important Notes
- Data is tenant-isolated by authenticated user.
- Most records belong to the currently logged-in user.
- `PlayerRanking` is not filtered the same way as other user-owned entities.
- Some lookup behavior intentionally bypasses tenant restrictions where required.

## Main API Summary

### Auth
- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/logout`
- `GET /api/auth/profile`
- `PUT /api/auth/profile`

### Members
- `POST /api/member`
- `GET /api/member`
- `GET /api/member/{id}`
- `GET /api/member/by-contact?contactValue=...`
- `GET /api/member/lookup?contactValue=...`
- `PUT /api/member/{id}`
- `DELETE /api/member/{id}`

### Sessions
- `POST /api/session`
- `GET /api/session`
- `GET /api/session/dashboard`
- `GET /api/session/{id}`
- `GET /api/session/{id}/detail`
- `PUT /api/session/{id}`
- `DELETE /api/session/{id}`

### Session Players
- `POST /api/sessionplayer`
- `GET /api/sessionplayer/{id}`
- `PATCH /api/sessionplayer/{id}/status`
- `DELETE /api/sessionplayer/{id}`

### Payments
- `POST /api/payment/session/{sessionId}`
- `POST /api/payment/session-player/{sessionPlayerId}/pay`
