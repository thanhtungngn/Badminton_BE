using System.ComponentModel;
using System.Text.Json;
using Badminton_MCP;
using ModelContextProtocol.Server;

namespace Badminton_MCP.Tools;

/// <summary>
/// MCP tools for interacting with the Trello board used to track Badminton project tasks.
/// Credentials (TRELLO_API_KEY, TRELLO_TOKEN) and optional TRELLO_MEMBER_ID / TRELLO_BOARD_ID
/// are read from environment variables.
/// </summary>
[McpServerToolType]
public sealed class TrelloTools
{
    private readonly TrelloClient _trello;

    public TrelloTools(TrelloClient trello)
    {
        _trello = trello;
    }

    [McpServerTool]
    [Description(
        "Returns all open Trello cards assigned to the configured member (TRELLO_MEMBER_ID env var) " +
        "that have a label whose name is 'AI' (case-insensitive). " +
        "Returns a JSON array with id, name, desc, shortUrl, idList, labels.")]
    public async Task<string> GetMyAICards()
    {
        var memberId = Environment.GetEnvironmentVariable("TRELLO_MEMBER_ID")
            ?? throw new InvalidOperationException("TRELLO_MEMBER_ID environment variable is not set.");

        var cards = await _trello.GetMemberCardsAsync(memberId);

        var aiCards = cards
            .Where(c => c.Labels.Any(l => string.Equals(l.Name.Trim(), "AI", StringComparison.OrdinalIgnoreCase)))
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Desc,
                c.ShortUrl,
                c.IdList,
                Labels = c.Labels.Select(l => new { l.Id, l.Name, l.Color }).ToList(),
            })
            .ToList();

        return JsonSerializer.Serialize(aiCards, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description(
        "Returns all open cards on the configured board (TRELLO_BOARD_ID env var) " +
        "assigned to the given memberId and having a label named 'AI'. " +
        "Pass an empty string for memberId to return all AI-labelled cards on the board.")]
    public async Task<string> GetBoardAICards(
        [Description("Trello member ID to filter by. Pass empty string to return all AI cards on the board.")] string memberId)
    {
        var boardId = Environment.GetEnvironmentVariable("TRELLO_BOARD_ID")
            ?? throw new InvalidOperationException("TRELLO_BOARD_ID environment variable is not set.");

        var cards = await _trello.GetBoardCardsAsync(boardId);

        var aiCards = cards
            .Where(c => c.Labels.Any(l => string.Equals(l.Name.Trim(), "AI", StringComparison.OrdinalIgnoreCase)))
            .Where(c => string.IsNullOrWhiteSpace(memberId) || c.IdMembers.Contains(memberId))
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Desc,
                c.ShortUrl,
                c.IdList,
                c.IdMembers,
                Labels = c.Labels.Select(l => new { l.Id, l.Name, l.Color }).ToList(),
            })
            .ToList();

        return JsonSerializer.Serialize(aiCards, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description(
        "Returns the lists (columns) on the configured board (TRELLO_BOARD_ID env var). " +
        "Useful for finding the ID of the 'Done' list to move completed cards into.")]
    public async Task<string> GetBoardLists()
    {
        var boardId = Environment.GetEnvironmentVariable("TRELLO_BOARD_ID")
            ?? throw new InvalidOperationException("TRELLO_BOARD_ID environment variable is not set.");

        var lists = await _trello.GetBoardListsAsync(boardId);
        return JsonSerializer.Serialize(lists, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description(
        "Moves a Trello card to the specified list (e.g., 'Done'). " +
        "Use GetBoardLists to find the target list ID.")]
    public async Task<string> MoveCardToList(
        [Description("The ID of the Trello card to move.")] string cardId,
        [Description("The ID of the destination Trello list.")] string listId)
    {
        await _trello.MoveCardToListAsync(cardId, listId);
        return $"Card {cardId} successfully moved to list {listId}.";
    }

    [McpServerTool]
    [Description("Adds a comment to a Trello card. Useful for posting a link to the GitHub PR that implements the card.")]
    public async Task<string> AddCommentToCard(
        [Description("The ID of the Trello card to comment on.")] string cardId,
        [Description("The comment text to add.")] string comment)
    {
        await _trello.AddCommentAsync(cardId, comment);
        return $"Comment added to card {cardId}.";
    }
}
