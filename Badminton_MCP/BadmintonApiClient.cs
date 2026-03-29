using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Badminton_MCP;

/// <summary>
/// HTTP client wrapper for the Badminton_BE REST API.
/// </summary>
public sealed class BadmintonApiClient
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _jsonOpts = new(JsonSerializerDefaults.Web);

    public string BaseUrl { get; }
    public string Token { get; private set; }

    public BadmintonApiClient(string baseUrl)
    {
        BaseUrl = baseUrl.TrimEnd('/');
        _http = new HttpClient { BaseAddress = new Uri(BaseUrl + "/") };
    }

    // ─── Auth ────────────────────────────────────────────────────────────────

    public void SetToken(string token)
    {
        Token = token;
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearToken()
    {
        Token = null;
        _http.DefaultRequestHeaders.Authorization = null;
    }

    // ─── Generic helpers ─────────────────────────────────────────────────────

    private async Task<(bool ok, string body)> SendAsync(HttpMethod method, string relativeUrl, object payload = null)
    {
        var request = new HttpRequestMessage(method, relativeUrl);
        if (payload != null)
            request.Content = new StringContent(
                JsonSerializer.Serialize(payload, _jsonOpts),
                Encoding.UTF8,
                "application/json");

        var response = await _http.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();
        return (response.IsSuccessStatusCode, body);
    }

    public Task<(bool ok, string body)> GetAsync(string url) =>
        SendAsync(HttpMethod.Get, url);

    public Task<(bool ok, string body)> PostAsync(string url, object payload = null) =>
        SendAsync(HttpMethod.Post, url, payload);

    public Task<(bool ok, string body)> PutAsync(string url, object payload) =>
        SendAsync(HttpMethod.Put, url, payload);

    public Task<(bool ok, string body)> PatchAsync(string url, object payload) =>
        SendAsync(HttpMethod.Patch, url, payload);

    public Task<(bool ok, string body)> DeleteAsync(string url) =>
        SendAsync(HttpMethod.Delete, url);
}
