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

## 3. MCP Project Management Server

**Workspace config:** `.vscode/mcp.json`
**Remote endpoint:** `https://project-management-mcp.onrender.com/mcp`

### What It Does

The team uses a remotely deployed MCP server for project-management operations in chat. This server provides Jira, Trello, and GitHub tools used by the agent workflows.

### Main Capabilities

| System | Typical Operations |
|---|---|
| Jira | list projects, search issues, read issue detail, add comments, transition status, create issues |
| Trello | list boards/lists/cards, read card detail, create/update/delete cards |
| GitHub | list repositories/issues/branches/commits, create/read issues |

### Configuration

Use workspace-level MCP config:

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

No local Badminton_MCP runtime is required for normal AI workflow.

---

---

## 4. Jira AI Agent

**Workflow:** `.github/workflows/jira-agent.yml`
**Dev instructions:** `.github/agents/jira-dev-agent.md`
**BA instructions:** `.github/agents/jira-ba-agent.md`

### What It Does

This agent scans your Jira Cloud project daily for tickets that are:
- Assigned to you (`JIRA_USER_EMAIL`)
- Tagged with the **AI** label
- Not yet in a "Done" status category

For each qualifying ticket it hasn't already processed, it:

1. **Checks description quality.** If the description is empty or fewer than 50 characters:
   - Calls the GitHub Models API (GPT-4o) to generate two draft descriptions from the ticket title — one from a **developer perspective** and one from a **business analyst perspective**.
   - Posts both drafts as a comment on the Jira ticket, asking the assignee to update the description.
   - Skips creating a GitHub Issue until the description is updated and the agent re-runs.

2. **Estimates effort.** If the ticket has a good description but no original time estimate:
   - Calls the GitHub Models API to estimate effort in hours.
   - Updates the Jira ticket's **Original Estimate** field.
   - Posts a comment explaining the reasoning behind the estimate.

3. **Creates a GitHub Issue** for the Copilot coding agent:
   - Embeds `jira-issue-key:<KEY>` in the body for deduplication.
   - Assigns the issue to `@copilot`.
   - Posts the GitHub Issue URL back as a comment on the Jira ticket.

### Two Agent Modes

| Agent | Instructions file | Purpose |
|---|---|---|
| **Dev agent** | `.github/agents/jira-dev-agent.md` | Implements code changes, creates PRs |
| **BA agent** | `.github/agents/jira-ba-agent.md` | Writes user stories and acceptance criteria |

The Copilot coding agent reads both files and uses them depending on whether the ticket requires code changes or documentation/requirements work.

### Trigger

- **Scheduled:** Every weekday at 08:30 UTC.
- **Manual:** Via GitHub Actions → *Jira AI Agent* → *Run workflow*.

### Required Secrets

| Secret | Description |
|---|---|
| `JIRA_BASE_URL` | Your Jira Cloud base URL, e.g. `https://your-org.atlassian.net` |
| `JIRA_USER_EMAIL` | The email address of the Jira account to authenticate as. |
| `JIRA_API_TOKEN` | A Jira API token for that account. |

### How to Set Up

1. Generate a Jira API token at [https://id.atlassian.com/manage-profile/security/api-tokens](https://id.atlassian.com/manage-profile/security/api-tokens).
2. Add the three secrets to **Settings → Secrets → Actions** in the GitHub repository.
3. Create a label named exactly **`AI`** in your Jira project.
4. Assign tickets with this label to yourself and they will be picked up automatically.

### Label Convention

Create a label named exactly **`AI`** (case-insensitive) in your Jira project. Tickets tagged with this label and assigned to you will be picked up.

---

## Summary

| Component | Trigger | Output |
|---|---|---|
| Trello AI Agent workflow | Schedule / manual | GitHub Issues assigned to Copilot |
| Jira AI Agent workflow | Schedule / manual | GitHub Issues assigned to Copilot + Jira comments & estimates |
| Copilot coding agent | Issue assigned to `@copilot` | Pull Request implementing the ticket |
| Release Notes Agent workflow | release/* → master PR merge | `docs/releases/<v>.md` + GitHub Release |
| Remote MCP server | MCP tool call | Jira/Trello/GitHub read/write via API |
