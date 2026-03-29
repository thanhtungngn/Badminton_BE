using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;

namespace Badminton_MCP.Tools;

/// <summary>
/// MCP tools for session match management.
/// </summary>
[McpServerToolType]
public sealed class SessionMatchTools(BadmintonApiClient api)
{
    /// <summary>
    /// Get all matches for a session.
    /// </summary>
    [McpServerTool, Description("Get all matches for a session.")]
    public async Task<string> GetMatches(
        [Description("Session ID")] int sessionId,
        CancellationToken ct = default)
    {
        var (ok, body) = await api.GetAsync($"api/session/{sessionId}/matches");
        return ok ? body : $"Error: {body}";
    }

    /// <summary>
    /// Get a single match by id.
    /// </summary>
    [McpServerTool, Description("Get a single match by session ID and match ID.")]
    public async Task<string> GetMatch(
        [Description("Session ID")] int sessionId,
        [Description("Match ID")] int matchId,
        CancellationToken ct = default)
    {
        var (ok, body) = await api.GetAsync($"api/session/{sessionId}/matches/{matchId}");
        return ok ? body : $"Error: {body}";
    }

    /// <summary>
    /// Create a match in a session.
    /// </summary>
    [McpServerTool, Description(
        "Create a match within a session. " +
        "teamAPlayerIds and teamBPlayerIds are JSON arrays of session-player IDs (1–2 players each). " +
        "winner is 'TeamA', 'TeamB', 'Draw', or null.")]
    public async Task<string> CreateMatch(
        [Description("Session ID")] int sessionId,
        [Description("JSON array of session-player IDs for team A, e.g. [1,2]")] string teamAPlayerIds,
        [Description("JSON array of session-player IDs for team B, e.g. [3,4]")] string teamBPlayerIds,
        [Description("Team A score (optional)")] int? teamAScore = null,
        [Description("Team B score (optional)")] int? teamBScore = null,
        [Description("Winner: TeamA, TeamB, Draw, or null")] string winner = null,
        CancellationToken ct = default)
    {
        int[] teamA, teamB;
        try
        {
            teamA = System.Text.Json.JsonSerializer.Deserialize<int[]>(teamAPlayerIds);
            teamB = System.Text.Json.JsonSerializer.Deserialize<int[]>(teamBPlayerIds);
        }
        catch (System.Text.Json.JsonException)
        {
            return "Error: teamAPlayerIds or teamBPlayerIds is not a valid JSON integer array.";
        }

        var dto = new { teamASessionPlayerIds = teamA, teamBSessionPlayerIds = teamB, teamAScore, teamBScore, winner };
        var (ok, body) = await api.PostAsync($"api/session/{sessionId}/matches", dto);
        return ok ? body : $"Error: {body}";
    }

    /// <summary>
    /// Update a match.
    /// </summary>
    [McpServerTool, Description("Update a match's teams, score, and winner.")]
    public async Task<string> UpdateMatch(
        [Description("Session ID")] int sessionId,
        [Description("Match ID")] int matchId,
        [Description("JSON array of session-player IDs for team A")] string teamAPlayerIds,
        [Description("JSON array of session-player IDs for team B")] string teamBPlayerIds,
        [Description("Team A score (optional)")] int? teamAScore = null,
        [Description("Team B score (optional)")] int? teamBScore = null,
        [Description("Winner: TeamA, TeamB, Draw, or null")] string winner = null,
        CancellationToken ct = default)
    {
        int[] teamA, teamB;
        try
        {
            teamA = System.Text.Json.JsonSerializer.Deserialize<int[]>(teamAPlayerIds);
            teamB = System.Text.Json.JsonSerializer.Deserialize<int[]>(teamBPlayerIds);
        }
        catch (System.Text.Json.JsonException)
        {
            return "Error: teamAPlayerIds or teamBPlayerIds is not a valid JSON integer array.";
        }

        var dto = new { teamASessionPlayerIds = teamA, teamBSessionPlayerIds = teamB, teamAScore, teamBScore, winner };
        var (ok, body) = await api.PutAsync($"api/session/{sessionId}/matches/{matchId}", dto);
        return ok ? body : $"Error: {body}";
    }

    /// <summary>
    /// Delete a match.
    /// </summary>
    [McpServerTool, Description("Delete a match by session ID and match ID.")]
    public async Task<string> DeleteMatch(
        [Description("Session ID")] int sessionId,
        [Description("Match ID")] int matchId,
        CancellationToken ct = default)
    {
        var (ok, body) = await api.DeleteAsync($"api/session/{sessionId}/matches/{matchId}");
        return ok ? "Match deleted." : $"Error: {body}";
    }
}
