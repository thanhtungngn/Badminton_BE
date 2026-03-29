using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;

namespace Badminton_MCP.Tools;

/// <summary>
/// MCP tools for payment management.
/// </summary>
[McpServerToolType]
public sealed class PaymentTools(BadmintonApiClient api)
{
    /// <summary>
    /// Set pricing for a session.
    /// </summary>
    [McpServerTool, Description("Set the male and female player prices for a session.")]
    public async Task<string> SetSessionPrices(
        [Description("Session ID")] int sessionId,
        [Description("Price for male players")] decimal priceMale,
        [Description("Price for female players")] decimal priceFemale,
        CancellationToken ct = default)
    {
        var dto = new { priceMale, priceFemale };
        var (ok, body) = await api.PostAsync($"api/payment/session/{sessionId}", dto);
        return ok ? body : $"Error: {body}";
    }

    /// <summary>
    /// Record a payment for a session player.
    /// </summary>
    [McpServerTool, Description("Record a payment for a session player by their session-player ID.")]
    public async Task<string> PaySessionPlayer(
        [Description("Session-player ID")] int sessionPlayerId,
        [Description("Amount paid")] decimal amount,
        CancellationToken ct = default)
    {
        var dto = new { amount };
        var (ok, body) = await api.PostAsync($"api/payment/session-player/{sessionPlayerId}/pay", dto);
        return ok ? body : $"Error: {body}";
    }
}
