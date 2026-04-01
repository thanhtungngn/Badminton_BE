using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace Badminton_MCP.Tools;

/// <summary>
/// MCP tools for managing a badminton club via the Badminton REST API.
/// Call Login() first if BADMINTON_API_TOKEN env var is not set.
/// </summary>
[McpServerToolType]
public sealed class BadmintonTools
{
    private readonly BadmintonApiClient _api;

    public BadmintonTools(BadmintonApiClient api) => _api = api;

    // -------------------------------------------------------------------------
    // AUTH
    // -------------------------------------------------------------------------

    [McpServerTool]
    [Description(
        "Authenticate with the Badminton API using username and password. " +
        "Must be called once per session if BADMINTON_API_TOKEN env var is not set. " +
        "The JWT token is stored automatically — no need to pass it to subsequent tools.")]
    public async Task<string> Login(
        [Description("Your Badminton account username.")] string username,
        [Description("Your Badminton account password.")] string password)
    {
        try
        {
            var json = await _api.PostAsync("api/auth/login", new { Username = username, Password = password });
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("token", out var tokenProp))
            {
                _api.SetToken(tokenProp.GetString() ?? string.Empty);
                var expires = root.TryGetProperty("expiresAt", out var exp) ? exp.GetString() : "unknown";
                var uname = root.TryGetProperty("username", out var un) ? un.GetString() : username;
                return $"Logged in as '{uname}'. Token stored for this session. Expires: {expires}";
            }

            return $"Login succeeded but response format was unexpected: {json}";
        }
        catch (HttpRequestException ex) { return $"Login failed: {ex.Message}"; }
    }

    // -------------------------------------------------------------------------
    // SESSIONS
    // -------------------------------------------------------------------------

    [McpServerTool]
    [Description(
        "Returns all sessions belonging to the authenticated user. " +
        "Includes session id, title, address, status (Upcoming/OnGoing/Ended), start/end times, courts, and capacity.")]
    public async Task<string> GetSessions()
    {
        try { return await _api.GetAsync("api/session"); }
        catch (HttpRequestException ex) { return $"Error: {ex.Message}"; }
    }

    [McpServerTool]
    [Description(
        "Returns only active sessions (Upcoming and OnGoing) for a quick dashboard view. " +
        "Use this to see what sessions are happening now or soon.")]
    public async Task<string> GetDashboardSessions()
    {
        try { return await _api.GetAsync("api/session/dashboard"); }
        catch (HttpRequestException ex) { return $"Error: {ex.Message}"; }
    }

    [McpServerTool]
    [Description(
        "Returns full detail for one session including: all players (name, elo, paid status, price) " +
        "and all matches (teams, scores, winner). Use this to get the complete picture of a session.")]
    public async Task<string> GetSessionDetail(
        [Description("The numeric session ID.")] int sessionId)
    {
        try { return await _api.GetAsync($"api/session/{sessionId}/detail"); }
        catch (HttpRequestException ex) { return $"Error: {ex.Message}"; }
    }

    [McpServerTool]
    [Description(
        "Creates a new badminton session. Returns the created session with its ID. " +
        "Pricing (priceMale / priceFemale) is set at creation time.")]
    public async Task<string> CreateSession(
        [Description("Session title, e.g. 'Sunday morning badminton'.")] string title,
        [Description("Venue address.")] string address,
        [Description("Start time in ISO 8601 format, e.g. '2025-08-10T08:00:00'.")] string startTime,
        [Description("End time in ISO 8601 format, e.g. '2025-08-10T11:00:00'.")] string endTime,
        [Description("Number of courts available (must be >= 1).")] int numberOfCourts,
        [Description("Price per male player in local currency.")] decimal priceMale,
        [Description("Price per female player in local currency.")] decimal priceFemale,
        [Description("Optional session description.")] string? description = null,
        [Description("Optional max players per court. Omit for unlimited.")] int? maxPlayerPerCourt = null)
    {
        try
        {
            return await _api.PostAsync("api/session", new
            {
                Title = title,
                Description = description,
                StartTime = DateTime.Parse(startTime),
                EndTime = DateTime.Parse(endTime),
                Address = address,
                NumberOfCourts = numberOfCourts,
                MaxPlayerPerCourt = maxPlayerPerCourt,
                PriceMale = priceMale,
                PriceFemale = priceFemale
            });
        }
        catch (Exception ex) { return $"Error: {ex.Message}"; }
    }

    [McpServerTool]
    [Description(
        "Updates an existing session. Changing status to 1 (OnGoing) automatically creates payment records for all current players. " +
        "Status: 0=Upcoming, 1=OnGoing, 2=Ended.")]
    public async Task<string> UpdateSession(
        [Description("The numeric session ID to update.")] int sessionId,
        [Description("Session title.")] string title,
        [Description("Venue address.")] string address,
        [Description("Start time in ISO 8601 format.")] string startTime,
        [Description("End time in ISO 8601 format.")] string endTime,
        [Description("Number of courts.")] int numberOfCourts,
        [Description("Session status: 0=Upcoming, 1=OnGoing, 2=Ended.")] int status,
        [Description("Optional description.")] string? description = null,
        [Description("Optional max players per court.")] int? maxPlayerPerCourt = null)
    {
        try
        {
            return await _api.PutAsync($"api/session/{sessionId}", new
            {
                Title = title,
                Description = description,
                StartTime = DateTime.Parse(startTime),
                EndTime = DateTime.Parse(endTime),
                Address = address,
                Status = status,
                NumberOfCourts = numberOfCourts,
                MaxPlayerPerCourt = maxPlayerPerCourt
            });
        }
        catch (Exception ex) { return $"Error: {ex.Message}"; }
    }

    // -------------------------------------------------------------------------
    // MEMBERS
    // -------------------------------------------------------------------------

    [McpServerTool]
    [Description(
        "Returns all club members with ranking (elo, tier), match stats (wins/losses/draws/winRate), " +
        "and contact information.")]
    public async Task<string> GetMembers()
    {
        try { return await _api.GetAsync("api/member"); }
        catch (HttpRequestException ex) { return $"Error: {ex.Message}"; }
    }

    [McpServerTool]
    [Description(
        "Returns a single member's full profile: contacts, ranking, match stats, and unpaid session debt.")]
    public async Task<string> GetMemberById(
        [Description("The numeric member ID.")] int memberId)
    {
        try { return await _api.GetAsync($"api/member/{memberId}"); }
        catch (HttpRequestException ex) { return $"Error: {ex.Message}"; }
    }

    [McpServerTool]
    [Description(
        "Public anonymous lookup — finds a member by phone number, email, or Facebook contact value. " +
        "Returns their sessions, payment status per session, and ranking. Does not require authentication.")]
    public async Task<string> LookupMemberByContact(
        [Description("Contact value to search by: phone number, email address, or Facebook name.")] string contactValue)
    {
        try { return await _api.GetAsync($"api/member/lookup?contactValue={Uri.EscapeDataString(contactValue)}"); }
        catch (HttpRequestException ex) { return $"Error: {ex.Message}"; }
    }

    // -------------------------------------------------------------------------
    // SESSION PLAYERS
    // -------------------------------------------------------------------------

    [McpServerTool]
    [Description(
        "Adds a member to a session. Returns the session-player record with its ID. " +
        "Fails if: session is full, member already in session, or member has an overlapping session.")]
    public async Task<string> AddMemberToSession(
        [Description("The numeric session ID.")] int sessionId,
        [Description("The numeric member ID to add.")] int memberId)
    {
        try
        {
            return await _api.PostAsync("api/sessionplayer", new { SessionId = sessionId, MemberId = memberId });
        }
        catch (HttpRequestException ex) { return $"Error: {ex.Message}"; }
    }

    [McpServerTool]
    [Description(
        "Removes a player from a session using their session-player record ID (not the member ID). " +
        "Use GetSessionDetail to find the correct session-player ID.")]
    public async Task<string> RemoveMemberFromSession(
        [Description("The session-player record ID (from GetSessionDetail player list).")] int sessionPlayerId)
    {
        try { return await _api.DeleteAsync($"api/sessionplayer/{sessionPlayerId}"); }
        catch (HttpRequestException ex) { return $"Error: {ex.Message}"; }
    }

    [McpServerTool]
    [Description(
        "Updates a player's status within a session. " +
        "Status: 0=Joined, 1=Canceled, 2=Paid, 3=NotPaid. " +
        "Use this to cancel a player, or to mark them paid after recording the payment separately.")]
    public async Task<string> UpdateSessionPlayerStatus(
        [Description("The session-player record ID.")] int sessionPlayerId,
        [Description("New status: 0=Joined, 1=Canceled, 2=Paid, 3=NotPaid.")] int status)
    {
        try
        {
            return await _api.PatchAsync(
                $"api/sessionplayer/{sessionPlayerId}/status",
                new { Status = status });
        }
        catch (HttpRequestException ex) { return $"Error: {ex.Message}"; }
    }

    // -------------------------------------------------------------------------
    // PAYMENTS
    // -------------------------------------------------------------------------

    [McpServerTool]
    [Description(
        "Sets or updates the male/female player pricing for a session. " +
        "If the session is already OnGoing, payment records are auto-created for all current players.")]
    public async Task<string> SetSessionPricing(
        [Description("The numeric session ID.")] int sessionId,
        [Description("Price per male player.")] decimal priceMale,
        [Description("Price per female player.")] decimal priceFemale)
    {
        try
        {
            return await _api.PostAsync(
                $"api/payment/session/{sessionId}",
                new { PriceMale = priceMale, PriceFemale = priceFemale });
        }
        catch (HttpRequestException ex) { return $"Error: {ex.Message}"; }
    }

    [McpServerTool]
    [Description(
        "Records a payment from a player for a session. " +
        "Use GetSessionDetail to find the session-player ID and the amount due for each player.")]
    public async Task<string> PaySessionPlayer(
        [Description("The session-player record ID.")] int sessionPlayerId,
        [Description("Amount the player is paying.")] decimal amount)
    {
        try
        {
            return await _api.PostAsync(
                $"api/payment/session-player/{sessionPlayerId}/pay",
                new { Amount = amount });
        }
        catch (HttpRequestException ex) { return $"Error: {ex.Message}"; }
    }
}
