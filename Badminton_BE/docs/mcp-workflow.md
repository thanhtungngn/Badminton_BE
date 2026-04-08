# MCP Workflow

## Purpose

The team uses a remote MCP server to let Copilot operate Jira/Trello/GitHub project workflows directly from chat.

- Remote endpoint: https://project-management-mcp.onrender.com/mcp
- Workspace config: .vscode/mcp.json
- Primary use cases:
  - discover assigned Jira/Trello work
  - comment and transition tickets/cards
  - create linked tracking artifacts

---

## Runtime Flow

```text
User prompt in Copilot Chat
  -> Copilot selects MCP tool
  -> HTTP call to remote MCP server (/mcp)
  -> Remote server calls Jira/Trello/GitHub APIs
  -> Result returned to Copilot
  -> Agent uses result to continue workflow
```

This is stateless HTTP tool invocation. No local MCP process is required for normal operation.

---

## Standard Task Loop (AI Ticket)

1. Discover work
- Ask for assigned AI tickets/cards.
- Agent uses Jira/Trello MCP read tools.

2. Analyze and plan
- Agent reviews code and ticket context.
- Agent posts BA/Dev clarification comments when needed.

3. Implement and validate
- Agent edits code, runs build/tests, updates docs.

4. Update tracking systems
- Agent posts PR URL to Jira/Trello.
- Agent transitions workflow status according to team rules.

---

## Configuration Checklist

1. Ensure .vscode/mcp.json contains the remote server URL.
2. In VS Code, run MCP: List Servers and verify badminton-mcp-remote is enabled.
3. Trust server if prompted.
4. Validate with a simple call in chat, for example: list Jira projects.

---

## Operational Notes

- The remote server owns Jira/Trello credentials.
- Repo-local MCP credentials are not required for daily AI work.
- If workflow tools fail, check remote MCP deployment status first.

---

## Troubleshooting

| Symptom | Likely Cause | Fix |
|---|---|---|
| No MCP tools visible | Server disabled/untrusted | Enable and trust badminton-mcp-remote in MCP: List Servers |
| Jira data not returned | Expired Jira credentials on remote | Update remote env vars and restart service |
| Trello updates fail | Invalid board/list/card permissions | Verify Trello token permissions on remote service |
| Slow first call | Service cold start | Retry after service wakes up |
