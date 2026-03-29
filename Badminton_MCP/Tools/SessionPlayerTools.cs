using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;

namespace Badminton_MCP.Tools;

/// <summary>
/// MCP tools for session-player management.
/// </summary>
[McpServerToolType]
public sealed class SessionPlayerTools(BadmintonApiClient api)
{
    /// <summary>
    /// Add a member to a session.
    /// </summary>
    [McpServerTool, Description("Add a member to a session. Default status is Joined.")]
    public async Task<string> AddMemberToSession(
        [Description("Session ID")] int sessionId,
        [Description("Member ID")] int memberId,
        CancellationToken ct = default)
    {
        var dto = new { sessionId, memberId };
        var (ok, body) = await api.PostAsync("api/sessionplayer", dto);
        return ok ? body : $"Error: {body}";
    }

    /// <summary>
    /// Get a session-player record by id.
    /// </summary>
    [McpServerTool, Description("Get a session-player record by its ID.")]
    public async Task<string> GetSessionPlayer(
        [Description("Session-player ID")] int id,
        CancellationToken ct = default)
    {
        var (ok, body) = await api.GetAsync($"api/sessionplayer/{id}");
        return ok ? body : $"Error: {body}";
    }

    /// <summary>
    /// Update a session-player's status.
    /// </summary>
    [McpServerTool, Description("Update the status of a session-player. Valid statuses: Joined, Canceled.")]
    public async Task<string> UpdateSessionPlayerStatus(
        [Description("Session-player ID")] int id,
        [Description("New status: Joined or Canceled")] string status,
        CancellationToken ct = default)
    {
        var dto = new { status };
        var (ok, body) = await api.PatchAsync($"api/sessionplayer/{id}/status", dto);
        return ok ? "Status updated." : $"Error: {body}";
    }

    /// <summary>
    /// Remove a member from a session.
    /// </summary>
    [McpServerTool, Description("Remove a member from a session by their session-player ID.")]
    public async Task<string> RemoveFromSession(
        [Description("Session-player ID")] int id,
        CancellationToken ct = default)
    {
        var (ok, body) = await api.DeleteAsync($"api/sessionplayer/{id}");
        return ok ? "Member removed from session." : $"Error: {body}";
    }
}
