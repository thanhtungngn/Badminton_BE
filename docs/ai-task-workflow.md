# AI Task Workflow

This document defines the end-to-end process for every task that comes from a Trello card tagged with the **AI** label.

---

## Rule Summary

| Step | Rule |
|------|------|
| Branch | `AI/<card-slug>` branched from `master` |
| Version | Bump patch number (`x.y.Z`) in both `.csproj` files before committing |
| Docs | Update `docs/version.md` with what changed; update `docs/current-state.md` if architecture changes |
| Commit | Single commit with message `feat|fix: <summary> (vX.Y.Z)` |
| PR | Opened from `AI/<slug>` → `master` via GitHub CLI |
| Trello | Card moved to **Code Review and Testing**; comment with PR URL and summary |

---

## Step-by-Step

### 1. Pick up the card
Use `GetMyAICards` or `GetBoardAICards` in Copilot Chat to list open AI cards.

```
"What AI Trello cards do I have?"
```

### 2. Create the branch
Run the start script — it pulls `master`, creates the branch, and checks it out.

```powershell
.\scripts\ai-start.ps1 -CardName "Bug - Session capacity not enforced"
# creates branch: AI/bug-session-capacity-not-enforced
```

Branch naming rules:
- Prefix: `AI/`
- Card name lowercased, spaces → hyphens, special chars stripped
- Example: `"Feature - Match History"` → `AI/feature-match-history`

### 3. Implement
Read the card description, explore the workspace, make the changes.

Required before committing:
- `docs/version.md` — add an entry under the current version (or create a new patch version block)
- `docs/current-state.md` — update if any capability, endpoint, or model changed

### 4. Bump version
Both `.csproj` files must share the same version. Bump only the patch digit.

```
1.1.0  →  1.1.1
1.1.1  →  1.1.2
```

Files to update:
- `Badminton_BE/Badminton_BE.csproj` → `<Version>X.Y.Z</Version>`
- `Badminton_MCP/Badminton_MCP.csproj` → `<Version>X.Y.Z</Version>`

### 5. Build and verify
```powershell
dotnet build
```
Do not proceed if the build fails.

### 6. Push and create PR
Run the finish script — it commits everything, pushes the branch, and opens a PR to `master`.

```powershell
.\scripts\ai-finish.ps1 `
  -CardId  "69cbf92d9aa2bae6bc674825" `
  -Summary "Fix session capacity not enforced when MaxPlayerPerCourt is set"
```

The script will:
1. Stage all changes (`git add -A`)
2. Commit with message: `fix: <summary> (vX.Y.Z)`
3. Push branch to origin
4. Create PR via `gh pr create` → `master`
5. Print the PR URL

### 7. Update Trello
The finish script also:
- Posts a comment on the card with the PR URL and summary
- Moves the card to **Code Review and Testing**

---

## Scripts Reference

| Script | Purpose |
|--------|---------|
| `scripts/ai-start.ps1` | Pull master, create and checkout `AI/<slug>` branch |
| `scripts/ai-finish.ps1` | Commit, push, create PR, update Trello card |

---

## Naming Examples

| Card name | Branch name |
|-----------|-------------|
| `Bug - Session capacity not enforced` | `AI/bug-session-capacity-not-enforced` |
| `Feature - Match History` | `AI/feature-match-history` |
| `Bug - API get members doesn't return stats` | `AI/bug-api-get-members-doesnt-return-stats` |

---

## Trello Board Lists (IDs)

| List | ID |
|------|----|
| Backlog | `69b1a58d14c314967d4f4877` |
| New | `69b053ecd1ed8f9bcd01e51c` |
| In Progress | `69b053ecd1ed8f9bcd01e51d` |
| Code Review and Testing | `69b053ecd1ed8f9bcd01e51e` |
| Done | `69b053ecd1ed8f9bcd01e51f` |
| Cancel | `69c68b2268b623f7f1f46b3c` |

---

## Environment Variables Required

The finish script reads Trello credentials from environment variables.
These are the same ones used in `.mcp.json` and `trello.runsettings`.

| Variable | Purpose |
|----------|---------|
| `TRELLO_API_KEY` | Trello REST API key |
| `TRELLO_TOKEN` | Trello OAuth token |
