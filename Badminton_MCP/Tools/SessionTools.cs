using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;

namespace Badminton_MCP.Tools;

/// <summary>
/// MCP tools for session management.
/// </summary>
[McpServerToolType]
public sealed class SessionTools(BadmintonApiClient api)
{
    /// <summary>
    /// Get all sessions.
    /// </summary>
    [McpServerTool, Description("Get all sessions.")]
    public async Task<string> GetSessions(CancellationToken ct = default)
    {
        var (ok, body) = await api.GetAsync("api/session");
        return ok ? body : $"Error: {body}";
    }

    /// <summary>
    /// Get active (upcoming + ongoing) sessions for the dashboard.
    /// </summary>
    [McpServerTool, Description("Get active (upcoming and ongoing) sessions for the dashboard.")]
    public async Task<string> GetActiveSessions(CancellationToken ct = default)
    {
        var (ok, body) = await api.GetAsync("api/session/dashboard");
        return ok ? body : $"Error: {body}";
    }

    /// <summary>
    /// Get a session by id.
    /// </summary>
    [McpServerTool, Description("Get a session by ID.")]
    public async Task<string> GetSession(
        [Description("Session ID")] int id,
        CancellationToken ct = default)
    {
        var (ok, body) = await api.GetAsync($"api/session/{id}");
        return ok ? body : $"Error: {body}";
    }

    /// <summary>
    /// Get detailed session info including players and matches.
    /// </summary>
    [McpServerTool, Description("Get detailed session info including registered players and match results.")]
    public async Task<string> GetSessionDetail(
        [Description("Session ID")] int id,
        CancellationToken ct = default)
    {
        var (ok, body) = await api.GetAsync($"api/session/{id}/detail");
        return ok ? body : $"Error: {body}";
    }

    /// <summary>
    /// Create a new session.
    /// </summary>
    [McpServerTool, Description("Create a new badminton session.")]
    public async Task<string> CreateSession(
        [Description("Session title")] string title,
        [Description("Start time in ISO 8601 format, e.g. 2026-04-01T09:00:00")] string startTime,
        [Description("End time in ISO 8601 format, e.g. 2026-04-01T11:00:00")] string endTime,
        [Description("Location (optional)")] string location = null,
        [Description("Max players (optional)")] int? maxPlayers = null,
        CancellationToken ct = default)
    {
        var dto = new { title, startTime, endTime, location, maxPlayers };
        var (ok, body) = await api.PostAsync("api/session", dto);
        return ok ? body : $"Error: {body}";
    }

    /// <summary>
    /// Update a session.
    /// </summary>
    [McpServerTool, Description("Update a session's details.")]
    public async Task<string> UpdateSession(
        [Description("Session ID")] int id,
        [Description("New title (optional)")] string title = null,
        [Description("New start time ISO 8601 (optional)")] string startTime = null,
        [Description("New end time ISO 8601 (optional)")] string endTime = null,
        [Description("New location (optional)")] string location = null,
        [Description("New status (optional): Upcoming, OnGoing, Completed, Canceled")] string status = null,
        [Description("New max players (optional)")] int? maxPlayers = null,
        CancellationToken ct = default)
    {
        var dto = new { title, startTime, endTime, location, status, maxPlayers };
        var (ok, body) = await api.PutAsync($"api/session/{id}", dto);
        return ok ? "Session updated." : $"Error: {body}";
    }

    /// <summary>
    /// Delete a session.
    /// </summary>
    [McpServerTool, Description("Delete a session by ID.")]
    public async Task<string> DeleteSession(
        [Description("Session ID")] int id,
        CancellationToken ct = default)
    {
        var (ok, body) = await api.DeleteAsync($"api/session/{id}");
        return ok ? "Session deleted." : $"Error: {body}";
    }

    /// <summary>
    /// Public registration — join a session by name, gender, and phone number.
    /// </summary>
    [McpServerTool, Description("Publicly register a player into a session using name, gender, and phone number. No authentication required.")]
    public async Task<string> RegisterPublic(
        [Description("Session ID")] int sessionId,
        [Description("Player name")] string name,
        [Description("Player gender: Male or Female")] string gender,
        [Description("Player phone number")] string phoneNumber,
        CancellationToken ct = default)
    {
        var dto = new { name, gender, phoneNumber };
        var (ok, body) = await api.PostAsync($"api/session/{sessionId}/register", dto);
        return ok ? body : $"Error: {body}";
    }
}
