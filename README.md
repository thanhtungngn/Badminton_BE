# Badminton_BE

A simple backend for managing badminton sessions, members and payments built with .NET 10 and Entity Framework Core.

## Features

- Manage sessions — create/update sessions, list sessions with players.
- Manage members — create, read, update members and contacts.
- Track session players — join/leave sessions and update status.
- Payments — create and track payments for members and sessions.
- EF Core migrations and database initialization via `scripts/run_migrations.sh`.
- `AppDbContextFactory` for design-time EF tooling.

## Requirements

- .NET 10 SDK
- Docker (optional, used by the migration script)
- Bash (for `scripts/run_migrations.sh`) — WSL or Git Bash on Windows if needed
- A relational DB (e.g. SQL Server, Postgres) — connection string configured in `ConnectionStrings__DefaultConnection`

## Quick start (local)

1. Restore and run
   - From repository root:
     - `cd Badminton_BE`
     - `dotnet restore`
     - `dotnet run`

2. Apply EF Core migrations (recommended before first run)
   - Using the included script (Linux/macOS/WSL/Git Bash):
     - `CONNECTION_STRING='Server=...;User Id=...;Password=...;Database=BadmintonDb' ./scripts/run_migrations.sh`
   - Or run directly:
     - `export ConnectionStrings__DefaultConnection='Server=...;User Id=...;Password=...;Database=BadmintonDb'`
     - `dotnet ef database update --project Badminton_BE --startup-project Badminton_BE`

3. Environment configuration
   - The app reads `ConnectionStrings__DefaultConnection` environment variable or `appsettings.json`.
   - Example (Linux/Mac):
     - `export ConnectionStrings__DefaultConnection='Server=localhost;Database=BadmintonDb;User Id=sa;Password=YourPassword;'`
   - On Windows PowerShell:
     - `$env:ConnectionStrings__DefaultConnection = 'Server=...;Database=...;User Id=...;Password=...'

## Docker

- Build image:
  - `docker build -t badminton-be .`
- Run container (set DB connection):
  - `docker run --rm -e ConnectionStrings__DefaultConnection="Server=...;Database=...;User Id=...;Password=..." -p 5000:80 badminton-be`

## Useful commands

- Run migrations (project-root):
  - `./scripts/run_migrations.sh` (with `CONNECTION_STRING` env)
- Direct EF command:
  - `dotnet ef migrations add <Name> --project Badminton_BE --startup-project Badminton_BE`
  - `dotnet ef database update --project Badminton_BE --startup-project Badminton_BE`
- Run the API:
  - `cd Badminton_BE && dotnet run`

## API endpoints (examples)

- Sessions: `/api/session`
- Members: `/api/member`
- Payments: `/api/payment`
- Session players: `/api/sessionplayer`

Example: create a member with `curl`:

`curl -X POST http://localhost:5000/api/member -H "Content-Type: application/json" -d '{"firstName":"Jane","lastName":"Doe","contact": {"email":"jane@example.com"}}'`

## MCP Server (Badminton_MCP)

`Badminton_MCP` is a companion [Model Context Protocol](https://modelcontextprotocol.io) server that lets AI assistants (Claude Desktop, Cline, Cursor, etc.) interact with the Badminton_BE API.

### Quick start

1. Start the Badminton_BE API first (see above).
2. Run the MCP server:
   ```bash
   cd Badminton_MCP
   BADMINTON_API_URL=http://localhost:5000 dotnet run
   ```

### Claude Desktop configuration

Add this to your `claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "badminton": {
      "command": "dotnet",
      "args": ["run", "--project", "/absolute/path/to/Badminton_MCP"],
      "env": {
        "BADMINTON_API_URL": "https://your-api-host"
      }
    }
  }
}
```

### Available tools

| Category | Tools |
|----------|-------|
| Auth | `Login`, `Logout`, `Register`, `GetProfile` |
| Members | `GetMembers`, `GetMember`, `GetMemberByContact`, `LookupMember`, `CreateMember`, `UpdateMember`, `DeleteMember` |
| Sessions | `GetSessions`, `GetActiveSessions`, `GetSession`, `GetSessionDetail`, `CreateSession`, `UpdateSession`, `DeleteSession`, `RegisterPublic` |
| Session Players | `AddMemberToSession`, `GetSessionPlayer`, `UpdateSessionPlayerStatus`, `RemoveFromSession` |
| Matches | `GetMatches`, `GetMatch`, `CreateMatch`, `UpdateMatch`, `DeleteMatch` |
| Payments | `SetSessionPrices`, `PaySessionPlayer` |

## Contributing

- Follow the existing code style in `Badminton_BE`.
- Run migrations after making model changes.
- Add unit/integration tests where appropriate.

## License

Add project license information here.
