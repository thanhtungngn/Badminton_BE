# MCP Server Integration

## Overview

This repository now uses an externally deployed MCP server for project management tasks.

- Server name: badminton-mcp-remote
- Transport: HTTP
- Endpoint: https://project-management-mcp.onrender.com/mcp
- Scope: Jira, Trello, and GitHub project-management operations

We no longer rely on running a local Badminton_MCP process for day-to-day AI workflow.

---

## Workspace Configuration

Configure MCP at workspace level in .vscode/mcp.json:

```json
{
  "servers": {
    "badminton-mcp-remote": {
      "type": "http",
      "url": "https://project-management-mcp.onrender.com/mcp"
    }
  }
}
```

Notes:
- VS Code discovers workspace MCP config from .vscode/mcp.json.
- After editing this file, restart MCP servers from Command Palette if tools do not refresh.

---

## Available Capabilities

The remote server exposes project-management tools for:

- Jira:
  - list projects
  - search issues
  - get issue details
  - add comments
  - transition issue status
  - create issues
- Trello:
  - list boards/lists/cards
  - get card details
  - create/update/delete cards
- GitHub:
  - list repositories/issues/branches/commits
  - create and read issues

These tools are used by the Jira and Trello AI workflows documented in this repository.

---

## Security Guidance

- Do not commit plaintext credentials into repository files.
- Prefer server-side secret management in the deployed MCP service.
- Keep workspace mcp.json limited to server URL and transport unless local overrides are required.

---

## Troubleshooting

| Symptom | Likely Cause | Fix |
|---|---|---|
| MCP tools do not appear in chat | Server not started/trusted in VS Code | Run MCP: List Servers, trust and start badminton-mcp-remote |
| Tool calls fail intermittently | Remote service cold start/network issue | Retry once; check server health and logs |
| Jira/Trello calls return auth errors | Remote server credentials expired | Rotate credentials in remote deployment and restart service |
| Configuration ignored | Wrong file location | Ensure config is in .vscode/mcp.json |

---

## Related Docs

- docs/mcp-workflow.md
- docs/agents.md
- .vscode/mcp.json
