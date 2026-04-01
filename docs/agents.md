# Agents Documentation

This document describes the automated agents used in the **Badminton_BE** project.

---

## 1. Trello AI Agent

**File:** `.github/workflows/trello-agent.yml`
**Instructions:** `.github/agents/trello-agent.md`

### What It Does

This agent reads your Trello board daily, finds all cards that are:
- Assigned to you (`TRELLO_MEMBER_ID`)
- Tagged with the **AI** label

For each qualifying card it hasn't already processed, it:
1. Creates a GitHub Issue in this repository with the card title and description.
2. Embeds the Trello card ID (`trello-card-id:<id>`) in the issue body for deduplication.
3. Assigns the issue to the **GitHub Copilot coding agent** (`@copilot`).

The Copilot coding agent then reads the issue, consults `.github/agents/trello-agent.md` for project conventions, implements the change, and opens a Pull Request.

### Trigger

- **Scheduled:** Every weekday at 08:00 UTC.
- **Manual:** Via the GitHub Actions → *Trello AI Agent* → *Run workflow* button.

### Required Secrets

| Secret | Description |
|---|---|
| `TRELLO_API_KEY` | Your Trello Power-Up API key. |
| `TRELLO_TOKEN` | Your Trello user token with read access. |
| `TRELLO_BOARD_ID` | The ID of the Trello board to scan. |
| `TRELLO_MEMBER_ID` | Your Trello member ID (the `id` field from `/1/members/me`). |

### How to Set Up

1. Go to [https://trello.com/power-ups/admin](https://trello.com/power-ups/admin) to obtain your API key.
2. Generate a token at: `https://trello.com/1/authorize?expiration=never&scope=read,write&response_type=token&key=<YOUR_API_KEY>`
3. Find your member ID: `curl "https://api.trello.com/1/members/me?key=<KEY>&token=<TOKEN>"` — use the `id` field.
4. Find your board ID from the board URL: `https://trello.com/b/<BOARD_ID>/...`
5. Add all four values as repository secrets in **Settings → Secrets → Actions**.

### Label Convention

Create a label named exactly **`AI`** (case-insensitive) on your Trello board. Cards tagged with this label and assigned to you will be picked up.

---

## 2. Release Notes Agent

**File:** `.github/workflows/release-notes-agent.yml`
**Instructions:** `.github/agents/release-notes-agent.md`

### What It Does

When a Pull Request from a `release/*` branch is merged into `master`, this agent:
1. Extracts the version number from the branch name (e.g., `release/v1.0.3` → `v1.0.3`).
2. Collects all commits since the previous release tag.
3. Collects all merged PRs since the previous release.
4. Generates structured Markdown release notes using the **GitHub Models API** (GPT-4o).
5. Falls back to a commit-categorisation formatter if the AI API is unavailable.
6. Commits the release notes to `docs/releases/<version>.md`.
7. Creates a **GitHub Release** tagged with the version.

### Trigger

Automatically fires when a PR with `head.ref` starting with `release/` is **merged** into `master`.

```yaml
on:
  pull_request:
    types: [closed]
    branches: [master]
```

### Required Permissions

The workflow uses the automatic `GITHUB_TOKEN` — no extra secrets required. Ensure the repository **Actions** setting allows workflows to write to contents (`Settings → Actions → Workflow permissions → Read and write`).

### Branch Convention

Release branches must follow the naming pattern:
```
release/v<MAJOR>.<MINOR>.<PATCH>
```
Examples: `release/v1.0.3`, `release/v2.0.0`

### Output

- **File committed:** `docs/releases/<version>.md`
- **GitHub Release created:** tagged `<version>`, targeting `master`, with the generated notes as the release body.

---

## 3. MCP Trello Tools

**Project:** `Badminton_MCP/`
**Tools file:** `Badminton_MCP/Tools/TrelloTools.cs`

### What It Does

The `Badminton_MCP` project is a [Model Context Protocol](https://modelcontextprotocol.io) server that exposes tools for AI assistants (such as GitHub Copilot) to interact with the Trello board and the Badminton REST API.

### Trello Tools

| Tool | Description |
|---|---|
| `GetMyAICards` | Returns all open Trello cards assigned to you with the AI label. |
| `GetBoardAICards` | Returns AI-labelled cards from the full board, optionally filtered by member. |
| `GetBoardLists` | Returns all lists (columns) on the board to find list IDs. |
| `MoveCardToList` | Moves a card to a target list (e.g., moves completed cards to "Done"). |
| `AddCommentToCard` | Posts a comment to a card (e.g., links to the GitHub PR). |

### Environment Variables

| Variable | Description |
|---|---|
| `BADMINTON_API_URL` | Base URL of the Badminton REST API. |
| `TRELLO_API_KEY` | Trello Power-Up API key. |
| `TRELLO_TOKEN` | Trello user token. |
| `TRELLO_MEMBER_ID` | Your Trello member ID. |
| `TRELLO_BOARD_ID` | The Trello board ID to use as default. |

### Running Locally

```bash
cd Badminton_MCP
export BADMINTON_API_URL=https://your-api.onrender.com
export TRELLO_API_KEY=your_key
export TRELLO_TOKEN=your_token
export TRELLO_MEMBER_ID=your_member_id
export TRELLO_BOARD_ID=your_board_id
dotnet run
```

The server communicates over **stdio** using the MCP protocol — connect it to any MCP-compatible host (GitHub Copilot, Claude, etc.).

---

## Summary

| Component | Trigger | Output |
|---|---|---|
| Trello AI Agent workflow | Schedule / manual | GitHub Issues assigned to Copilot |
| Copilot coding agent | Issue assigned to `@copilot` | Pull Request implementing the card |
| Release Notes Agent workflow | release/* → master PR merge | `docs/releases/<v>.md` + GitHub Release |
| Badminton_MCP TrelloTools | MCP tool call | Trello read/write via API |
