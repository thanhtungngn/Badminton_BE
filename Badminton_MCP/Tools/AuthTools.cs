using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;

namespace Badminton_MCP.Tools;

/// <summary>
/// MCP tools for authentication (login / logout).
/// </summary>
[McpServerToolType]
public sealed class AuthTools(BadmintonApiClient api)
{
    /// <summary>
    /// Login with username and password. Returns a JWT token and stores it for
    /// subsequent authenticated calls.
    /// </summary>
    [McpServerTool, Description("Login to the Badminton API. Returns a JWT token and stores it for subsequent calls.")]
    public async Task<string> Login(
        [Description("Username")] string username,
        [Description("Password")] string password,
        CancellationToken ct = default)
    {
        var (ok, body) = await api.PostAsync("api/auth/login", new { username, password });
        if (!ok)
            return $"Login failed: {body}";

        // Extract the token from the response JSON and persist it.
        using var doc = System.Text.Json.JsonDocument.Parse(body);
        if (doc.RootElement.TryGetProperty("token", out var tokenProp))
        {
            api.SetToken(tokenProp.GetString());
            return "Login successful. Token stored.";
        }

        return $"Login succeeded but token was not found in response: {body}";
    }

    /// <summary>
    /// Logout from the Badminton API and revoke the stored JWT token.
    /// </summary>
    [McpServerTool, Description("Logout from the Badminton API and revoke the current JWT token.")]
    public async Task<string> Logout(CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(api.Token))
            return "Not logged in.";

        var (ok, body) = await api.PostAsync("api/auth/logout");
        api.ClearToken();

        return ok ? "Logged out successfully." : $"Logout failed: {body}";
    }

    /// <summary>
    /// Get the current authenticated user's profile.
    /// </summary>
    [McpServerTool, Description("Get the current authenticated user's profile.")]
    public async Task<string> GetProfile(CancellationToken ct = default)
    {
        var (ok, body) = await api.GetAsync("api/auth/profile");
        return ok ? body : $"Error: {body}";
    }

    /// <summary>
    /// Register a new account.
    /// </summary>
    [McpServerTool, Description("Register a new account.")]
    public async Task<string> Register(
        [Description("Username")] string username,
        [Description("Password")] string password,
        CancellationToken ct = default)
    {
        var (ok, body) = await api.PostAsync("api/auth/register", new { username, password });
        return ok ? body : $"Error: {body}";
    }
}
