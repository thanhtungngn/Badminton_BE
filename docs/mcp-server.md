# Badminton MCP Server

## Overview

`Badminton_MCP` is a [Model Context Protocol (MCP)](https://modelcontextprotocol.io/) server built with .NET 10.
It exposes tools that allow AI assistants (such as GitHub Copilot, Claude Desktop, or any MCP-compatible client) to interact with:

- **Trello** – read and manage project cards on the Badminton Trello board
- **Badminton REST API** – interact with the `Badminton_BE` backend (sessions, members, payments, etc.)

The server communicates over **standard I/O (stdio)**, which is the default transport for MCP servers used by most AI clients.

---

## Architecture

```
AI Client (Copilot / Claude / etc.)
        │  stdio
        ▼
  Badminton_MCP (MCP Server)
  ├── Tools/TrelloTools.cs      ← Trello board tools
  ├── TrelloClient.cs           ← Trello REST API wrapper
  └── BadmintonApiClient.cs     ← Badminton REST API wrapper (tools TBD)
```

### Key Files

| File | Purpose |
|------|---------|
| `Program.cs` | Bootstraps the MCP host, registers DI services, wires up stdio transport |
| `TrelloClient.cs` | Lightweight wrapper for Trello REST API v1 |
| `BadmintonApiClient.cs` | Generic HTTP client for the Badminton backend REST API |
| `Tools/TrelloTools.cs` | MCP tool definitions for Trello operations |

---

## Environment Variables

The server reads all credentials and configuration from environment variables at startup.

| Variable | Required | Description |
|----------|----------|-------------|
| `TRELLO_API_KEY` | Yes (Trello tools) | Trello REST API key |
| `TRELLO_TOKEN` | Yes (Trello tools) | Trello OAuth token |
| `TRELLO_MEMBER_ID` | Yes (GetMyAICards) | Trello member ID to filter cards by |
| `TRELLO_BOARD_ID` | Yes (GetBoardAICards, GetBoardLists) | Trello board ID to query |
| `BADMINTON_API_URL` | Yes (Badminton tools) | Base URL of the `Badminton_BE` API (e.g. `https://your-api.onrender.com`) |
| `BADMINTON_API_TOKEN` | Recommended | JWT bearer token for authenticated Badminton API calls |

---

## Available MCP Tools

### Trello Tools (`TrelloTools`)

#### `GetMyAICards`
Returns all open Trello cards assigned to the configured member (`TRELLO_MEMBER_ID`) that have a label named **AI**.

**Parameters:** none

**Returns:** JSON array with `id`, `name`, `desc`, `shortUrl`, `idList`, `labels`

---

#### `GetBoardAICards`
Returns all open cards on the configured board (`TRELLO_BOARD_ID`) that have a label named **AI**.
Optionally filters by a specific member.

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `memberId` | string | Trello member ID to filter by. Pass empty string to return all AI cards on the board. |

**Returns:** JSON array with `id`, `name`, `desc`, `shortUrl`, `idList`, `idMembers`, `labels`

---

#### `GetBoardLists`
Returns all lists (columns) on the configured board. Useful for finding the ID of the **Done** list to move completed cards into.

**Parameters:** none

**Returns:** JSON array with `id`, `name`

---

#### `MoveCardToList`
Moves a Trello card to a specified list.

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `cardId` | string | ID of the card to move |
| `listId` | string | ID of the destination list |

---

#### `AddCommentToCard`
Adds a comment to a Trello card. Useful for posting a link to the GitHub PR that implements the card.

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `cardId` | string | ID of the card |
| `comment` | string | Comment text to add |

---

## How to Run Locally

### 1. Set environment variables

```powershell
$env:TRELLO_API_KEY      = "your-trello-api-key"
$env:TRELLO_TOKEN        = "your-trello-token"
$env:TRELLO_MEMBER_ID    = "your-trello-member-id"
$env:TRELLO_BOARD_ID     = "your-trello-board-id"
$env:BADMINTON_API_URL   = "https://your-api.onrender.com"
$env:BADMINTON_API_TOKEN = "your-jwt-token"
```

### 2. Build and run

```powershell
cd Badminton_MCP
dotnet build
dotnet run
```

The server starts and listens on stdio — it is meant to be launched by an MCP client, not run interactively.

---

## Configuring with an MCP Client

### Visual Studio / GitHub Copilot (`.mcp.json`)

Create or update `.mcp.json` in the repository root:

```json
{
  "mcpServers": {
    "badminton-mcp": {
      "type": "stdio",
      "command": "dotnet",
      "args": ["run", "--project", "Badminton_MCP/Badminton_MCP.csproj"],
      "env": {
        "TRELLO_API_KEY": "${TRELLO_API_KEY}",
        "TRELLO_TOKEN": "${TRELLO_TOKEN}",
        "TRELLO_MEMBER_ID": "${TRELLO_MEMBER_ID}",
        "TRELLO_BOARD_ID": "${TRELLO_BOARD_ID}",
        "BADMINTON_API_URL": "${BADMINTON_API_URL}",
        "BADMINTON_API_TOKEN": "${BADMINTON_API_TOKEN}"
      }
    }
  }
}
```

### Claude Desktop (`claude_desktop_config.json`)

```json
{
  "mcpServers": {
    "badminton-mcp": {
      "command": "dotnet",
      "args": ["run", "--project", "D:\\SuNingBadminton\\Backend\\Badminton_MCP\\Badminton_MCP.csproj", "--no-build"],
      "env": {
        "TRELLO_API_KEY": "your-trello-api-key",
        "TRELLO_TOKEN": "your-trello-token",
        "TRELLO_MEMBER_ID": "your-member-id",
        "TRELLO_BOARD_ID": "your-board-id",
        "BADMINTON_API_URL": "https://your-api.onrender.com",
        "BADMINTON_API_TOKEN": "your-jwt-token"
      }
    }
  }
}
```

---

## What's Planned Next

See the [Next Steps](#next-steps) section below for the current development roadmap for this project.

### Badminton API Tools (not yet implemented)

The `BadmintonApiClient` is already registered in DI but no MCP tools use it yet.
The planned `Tools/BadmintonTools.cs` will expose:

| Planned Tool | Badminton API Endpoint |
|---|---|
| `GetSessions` | `GET /api/session` |
| `GetDashboardSessions` | `GET /api/session/dashboard` |
| `GetSessionDetail` | `GET /api/session/{id}/detail` |
| `GetMembers` | `GET /api/member` |
| `GetMemberById` | `GET /api/member/{id}` |
| `LookupMemberByContact` | `GET /api/member/lookup?contactValue=...` |
| `GetSessionMatches` | `GET /api/session/{sessionId}/matches` |
| `SetSessionPricing` | `POST /api/payment/session/{sessionId}` |
| `PaySessionPlayer` | `POST /api/payment/session-player/{sessionPlayerId}/pay` |

---

## Next Steps

In priority order:

1. **Add `BADMINTON_API_TOKEN` support to `BadmintonApiClient`** — read the JWT token from env var automatically so tools don't need to pass it manually each call.
2. **Create `Tools/BadmintonTools.cs`** — implement MCP tools for sessions, members, payments, and matches using `BadmintonApiClient`.
3. **Extend `BadmintonApiClient`** — add `PutAsync`, `PatchAsync`, and `DeleteAsync` methods to support update and delete operations.
4. **Add `.mcp.json`** to the repository root so the MCP server is auto-discovered by GitHub Copilot in VS / VS Code.
5. **Add more Trello tools** if needed — e.g., `CreateCard`, `UpdateCardDescription`.

---

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| `ModelContextProtocol` | 1.2.0 | MCP server framework |
| `Microsoft.Extensions.Hosting` | 10.0.5 | Generic host / DI container |
| `Microsoft.Extensions.Http` | 10.0.5 | `IHttpClientFactory` |
