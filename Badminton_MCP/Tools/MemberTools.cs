using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;

namespace Badminton_MCP.Tools;

/// <summary>
/// MCP tools for member management.
/// </summary>
[McpServerToolType]
public sealed class MemberTools(BadmintonApiClient api)
{
    /// <summary>
    /// Get all members.
    /// </summary>
    [McpServerTool, Description("Get all members with their contacts, level, Elo, and ranking.")]
    public async Task<string> GetMembers(CancellationToken ct = default)
    {
        var (ok, body) = await api.GetAsync("api/member");
        return ok ? body : $"Error: {body}";
    }

    /// <summary>
    /// Get a member by id.
    /// </summary>
    [McpServerTool, Description("Get a member by their ID.")]
    public async Task<string> GetMember(
        [Description("Member ID")] int id,
        CancellationToken ct = default)
    {
        var (ok, body) = await api.GetAsync($"api/member/{id}");
        return ok ? body : $"Error: {body}";
    }

    /// <summary>
    /// Get a member by a contact value (phone or email).
    /// </summary>
    [McpServerTool, Description("Get a member by a contact value such as phone number or email.")]
    public async Task<string> GetMemberByContact(
        [Description("Contact value (phone number or email)")] string contactValue,
        CancellationToken ct = default)
    {
        var (ok, body) = await api.GetAsync($"api/member/by-contact?contactValue={Uri.EscapeDataString(contactValue)}");
        return ok ? body : $"Error: {body}";
    }

    /// <summary>
    /// Anonymous lookup of a member's sessions, payment status, Elo, and level.
    /// </summary>
    [McpServerTool, Description("Anonymous lookup of a member's sessions, payment status, Elo, and level by contact value.")]
    public async Task<string> LookupMember(
        [Description("Contact value (phone number or email)")] string contactValue,
        CancellationToken ct = default)
    {
        var (ok, body) = await api.GetAsync($"api/member/lookup?contactValue={Uri.EscapeDataString(contactValue)}");
        return ok ? body : $"Error: {body}";
    }

    /// <summary>
    /// Create a new member.
    /// </summary>
    [McpServerTool, Description("Create a new member. Contacts is an optional JSON array of {type, value} objects.")]
    public async Task<string> CreateMember(
        [Description("Member name")] string name,
        [Description("Gender: Male or Female")] string gender,
        [Description("Optional level (e.g. Beginner, Intermediate, Advanced)")] string level = null,
        [Description("Optional JSON array of contacts, e.g. [{\"type\":\"Phone\",\"value\":\"0123456789\"}]")] string contactsJson = null,
        CancellationToken ct = default)
    {
        object contacts = null;
        if (!string.IsNullOrWhiteSpace(contactsJson))
        {
            try { contacts = System.Text.Json.JsonSerializer.Deserialize<object[]>(contactsJson); }
            catch (System.Text.Json.JsonException) { return "Error: contactsJson is not valid JSON."; }
        }

        var dto = new { name, gender, level, contacts };
        var (ok, body) = await api.PostAsync("api/member", dto);
        return ok ? body : $"Error: {body}";
    }

    /// <summary>
    /// Update a member.
    /// </summary>
    [McpServerTool, Description("Update a member's name, gender, level, and contacts.")]
    public async Task<string> UpdateMember(
        [Description("Member ID")] int id,
        [Description("New name")] string name = null,
        [Description("New gender: Male or Female")] string gender = null,
        [Description("New level")] string level = null,
        [Description("Optional JSON array of contacts to replace existing ones")] string contactsJson = null,
        CancellationToken ct = default)
    {
        object contacts = null;
        if (!string.IsNullOrWhiteSpace(contactsJson))
        {
            try { contacts = System.Text.Json.JsonSerializer.Deserialize<object[]>(contactsJson); }
            catch (System.Text.Json.JsonException) { return "Error: contactsJson is not valid JSON."; }
        }

        var dto = new { name, gender, level, contacts };
        var (ok, body) = await api.PutAsync($"api/member/{id}", dto);
        return ok ? "Member updated." : $"Error: {body}";
    }

    /// <summary>
    /// Delete a member.
    /// </summary>
    [McpServerTool, Description("Delete a member by ID.")]
    public async Task<string> DeleteMember(
        [Description("Member ID")] int id,
        CancellationToken ct = default)
    {
        var (ok, body) = await api.DeleteAsync($"api/member/{id}");
        return ok ? "Member deleted." : $"Error: {body}";
    }
}
