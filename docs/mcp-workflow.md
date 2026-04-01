# MCP Server Workflow

## What Is the MCP Server For?

The `Badminton_MCP` server bridges **GitHub Copilot** (or any MCP-compatible AI client) with two external systems:

| System | Purpose in This Project |
|--------|------------------------|
| **Trello** | Track feature cards labelled **AI** — tasks that Copilot is expected to implement |
| **Badminton REST API** | Let Copilot read and write live session/member/payment data *(tools planned)* |

Without the MCP server, Copilot can only read code in the workspace. With it, Copilot can also **read Trello cards**, **move them**, **post PR links**, and eventually **query the live backend** — all from a single chat conversation.

---

## How stdio Transport Works

The server is spawned **once per VS session** — not per prompt. GitHub Copilot starts it on VS startup, performs a handshake to discover available tools, then reuses the same process for every tool call during that session.

```
VS 2026 opens
    │
    ├── reads .mcp.json
    └── spawns process (ONCE): dotnet run Badminton_MCP --no-build
              │
              │  JSON-RPC handshake
              ├──► initialize
              ├──► tools/list
              │       server announces: GetMyAICards, GetBoardAICards,
              │                         GetBoardLists, MoveCardToList,
              │                         AddCommentToCard
              │
              │  ┌── User prompt #1 ────────────────────────────────┐
              │  │   Copilot  ──► tools/call GetMyAICards (stdin)   │
              │  │   Server   ──► Trello API                        │
              │  │   Server   ◄── JSON result                       │
              │  │   Copilot  ◄── result (stdout)                   │
              │  └──────────────────────────────────────────────────┘
              │
              │  ┌── User prompt #2 ────────────────────────────────┐
              │  │   Copilot  ──► tools/call MoveCardToList (stdin) │
              │  │   (same process — TrelloClient still alive)      │
              │  └──────────────────────────────────────────────────┘
              │
VS closes → server process exits
```

Key points:
- `.mcp.json` at the repo root tells VS 2026 how to start the server and which env vars to pass.
- All communication is **JSON-RPC 2.0 over stdin/stdout** — no ports, no HTTP listener.
- The server process is **long-lived** — `TrelloClient` and `HttpClient` are singletons, created once and reused across every tool call in the session.
- Copilot reads each tool's `[Description]` attribute to decide which tool to call for a given prompt.

## How Copilot Selects a Tool

Copilot never hard-codes which tool to call. It reads the `[Description]` attribute on each `[McpServerTool]` method and uses that text as the tool's contract with the AI model.

```csharp
[McpServerTool]
[Description(
    "Returns all open Trello cards assigned to the configured member (TRELLO_MEMBER_ID env var) " +
    "that have a label whose name is 'AI' (case-insensitive). " +
    "Returns a JSON array with id, name, desc, shortUrl, idList, labels.")]
public async Task<string> GetMyAICards() { ... }
```

When the user says *"What AI tasks do I have?"*, the model matches that intent to this description and emits a `tools/call` for `GetMyAICards`. A vague or missing description causes the wrong tool to be called (or none at all).

---

The workflow depends on the Trello board having a consistent structure:

```
Board: SuNing Badminton
│
├── 📋 Backlog          ← new cards start here
├── 🔄 In Progress      ← card moves here when work starts  
├── 👁  Review          ← card moves here when PR is opened
└── ✅ Done             ← card moves here after merge
```

### The AI Label

Cards tagged with the **AI** label (case-insensitive) are the ones Copilot is expected to work on. Any card without this label is ignored by the MCP tools.

```
Card: "Add BadmintonTools MCP integration"
  Labels: [AI, Backend]        ← Copilot will see this card
  
Card: "Update README"
  Labels: [Docs]               ← Copilot will NOT see this card
```

---

## Core Workflow: AI-Driven Feature Implementation

This is the main day-to-day loop — from picking up a card to closing it.

```
┌──────────────────────────────────────────────────────────────────┐
│  STEP 1 — Discover work                                          │
│                                                                  │
│  Prompt: "What AI tasks are assigned to me?"                     │
│                                                                  │
│  Copilot calls: GetMyAICards()                                   │
│    → TrelloClient.GetMemberCardsAsync(TRELLO_MEMBER_ID)          │
│    → filters cards where label == "AI"                           │
│    → returns: [{id, name, desc, shortUrl, idList, labels}]       │
└──────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌──────────────────────────────────────────────────────────────────┐
│  STEP 2 — Understand the task                                    │
│                                                                  │
│  Copilot reads the card's name and desc from Step 1.             │
│  Copilot reads relevant source files from the workspace.         │
│  Copilot produces an implementation plan.                        │
└──────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌──────────────────────────────────────────────────────────────────┐
│  STEP 3 — Implement                                              │
│                                                                  │
│  Copilot edits files, creates new files, writes tests.           │
│  All changes are inside the workspace — no MCP calls needed.     │
└──────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌──────────────────────────────────────────────────────────────────┐
│  STEP 4 — Post PR link as comment                                │
│                                                                  │
│  Prompt: "Add PR link https://github.com/.../pull/42 to card"   │
│                                                                  │
│  Copilot calls: AddCommentToCard(cardId, prUrl)                  │
│    → TrelloClient.AddCommentAsync(cardId, text)                  │
│    → POST /cards/{cardId}/actions/comments                       │
└──────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌──────────────────────────────────────────────────────────────────┐
│  STEP 5 — Move card to Done                                      │
│                                                                  │
│  Prompt: "Move this card to Done"                                │
│                                                                  │
│  Copilot calls: GetBoardLists()   ← finds the "Done" list ID     │
│    → TrelloClient.GetBoardListsAsync(boardId)                    │
│                                                                  │
│  Copilot calls: MoveCardToList(cardId, doneListId)               │
│    → TrelloClient.MoveCardToListAsync(cardId, listId)            │
│    → PUT /cards/{cardId}  { idList: doneListId }                 │
└──────────────────────────────────────────────────────────────────┘
```

---

## Tool Call Reference

### Tool: `GetMyAICards`

**When to use:** Start of session — "What should I work on today?"

```
Copilot prompt  →  GetMyAICards()
                       │
                       ▼
               GET /members/{TRELLO_MEMBER_ID}/cards
                   ?filter=open&fields=id,name,desc,labels,shortUrl,idList,idBoard
                       │
                       ▼
               filter: label.name == "AI"
                       │
                       ▼
               returns: JSON array of AI cards assigned to me
```

---

### Tool: `GetBoardAICards`

**When to use:** See all AI tasks on the board, not just mine — "What AI tasks are open for the whole team?"

```
Copilot prompt  →  GetBoardAICards(memberId: "" | "specific-id")
                       │
                       ▼
               GET /boards/{TRELLO_BOARD_ID}/cards
                   ?filter=open&fields=id,name,desc,labels,shortUrl,idList,idMembers
                       │
                       ▼
               filter: label.name == "AI"
                       AND (memberId is empty OR card.idMembers contains memberId)
                       │
                       ▼
               returns: JSON array of board-wide AI cards
```

---

### Tool: `GetBoardLists`

**When to use:** Before moving a card — need to resolve the "Done" list ID.

```
Copilot prompt  →  GetBoardLists()
                       │
                       ▼
               GET /boards/{TRELLO_BOARD_ID}/lists?fields=id,name
                       │
                       ▼
               returns: [{id: "abc", name: "Backlog"}, {id: "xyz", name: "Done"}, ...]
```

---

### Tool: `MoveCardToList`

**When to use:** After merging a PR — close out the card.

```
Copilot prompt  →  MoveCardToList(cardId, listId)
                       │
                       ▼
               PUT /cards/{cardId}  body: { idList: listId }
                       │
                       ▼
               returns: "Card {cardId} successfully moved to list {listId}."
```

---

### Tool: `AddCommentToCard`

**When to use:** After opening a PR — attach the link to the card for traceability.

```
Copilot prompt  →  AddCommentToCard(cardId, comment)
                       │
                       ▼
               POST /cards/{cardId}/actions/comments  body: { text: comment }
                       │
                       ▼
               returns: "Comment added to card {cardId}."
```

---

## Full Session Example

A complete conversation showing all tools in one session:

```
User:    "What AI Trello cards are assigned to me?"

Copilot: [calls GetMyAICards]
         Found 2 cards:
         1. "Add BadmintonTools MCP integration" — In Progress
         2. "Add JWT auth to BadmintonApiClient" — Backlog

User:    "Let's work on card 2. Implement it."

Copilot: [reads workspace files: BadmintonApiClient.cs, Program.cs, ...]
         [edits BadmintonApiClient.cs to read BADMINTON_API_TOKEN from env]
         [edits version.md to note the change]
         Done. Here's what changed: ...

User:    "I pushed a PR: https://github.com/thanhtungngn/Badminton_BE/pull/15
          Add that as a comment and move the card to Done."

Copilot: [calls AddCommentToCard(cardId2, "https://github.com/.../pull/15")]
         Comment added.

         [calls GetBoardLists]
         Lists: Backlog, In Progress, Review, Done (id: "xyz789")

         [calls MoveCardToList(cardId2, "xyz789")]
         Card moved to Done. ✅
```

---

## Planned Workflow: Badminton API Tools

Once `Tools/BadmintonTools.cs` is implemented, a second workflow becomes available — querying live session and payment data without leaving the chat.

```
┌──────────────────────────────────────────────────────────────────┐
│  PLANNED — Badminton API Workflow                                │
│                                                                  │
│  Prompt: "Who hasn't paid for the last session?"                 │
│                                                                  │
│  Copilot calls: GetDashboardSessions()                           │
│    → BadmintonApiClient.GetAsync("/api/session/dashboard")       │
│    → finds the most recent session                               │
│                                                                  │
│  Copilot calls: GetSessionDetail(sessionId)                      │
│    → BadmintonApiClient.GetAsync("/api/session/{id}/detail")     │
│    → filters players where paid == false                         │
│    → returns names and contact info                              │
└──────────────────────────────────────────────────────────────────┘
```

See `docs/mcp-server.md` → *What's Planned Next* for the full list of planned Badminton API tools.

---

## Setup Checklist

Use this before the first session or after a fresh clone:

```
□ 1. Build the MCP project
      dotnet build Badminton_MCP/Badminton_MCP.csproj -c Release

□ 2. Copy .mcp.json.example → .mcp.json
      Fill in TRELLO_API_KEY, TRELLO_TOKEN, TRELLO_BOARD_ID, TRELLO_MEMBER_ID

□ 3. Restart Visual Studio 2026
      VS reads .mcp.json on startup and registers the server with Copilot.

□ 4. Verify in Copilot Chat
      Ask: "What AI Trello cards do I have?"
      Expected: list of your AI-labelled cards (or empty list if none assigned).

□ 5. (Optional) Run integration tests to confirm credentials are valid
      Copy trello.runsettings.example → trello.runsettings
      Fill in credentials
      In VS: Test > Configure Run Settings > select trello.runsettings
      Run tests in Badminton_MCP.Tests — all 4 should pass.
```

---

## Troubleshooting

| Symptom | Likely Cause | Fix |
|---------|-------------|-----|
| Copilot doesn't see MCP tools | `.mcp.json` not found or malformed | Check file is at repo root; validate JSON syntax |
| Tools appear but return errors | Wrong credentials in `.mcp.json` | Run integration tests first to validate credentials |
| `GetMyAICards` returns empty | No cards labelled **AI** assigned to your member ID | Check Trello board — add AI label to a card and assign it |
| Server starts then crashes | `TRELLO_API_KEY` or `TRELLO_TOKEN` env var missing | Ensure all required keys are set in `.mcp.json` |
| Slow tool response | Project not pre-built; `dotnet run` is rebuilding | Run `dotnet build -c Release` then restart VS |
