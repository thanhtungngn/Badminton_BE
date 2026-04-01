using System.Net.Http.Headers;
using System.Text.Json;

namespace Badminton_MCP;

/// <summary>
/// HTTP client that wraps the Badminton REST API.
/// The API base URL is read from the BADMINTON_API_URL environment variable.
/// </summary>
public sealed class BadmintonApiClient
{
    private readonly HttpClient _http;

    public BadmintonApiClient(IHttpClientFactory httpClientFactory)
    {
        var baseUrl = Environment.GetEnvironmentVariable("BADMINTON_API_URL")
            ?? throw new InvalidOperationException("BADMINTON_API_URL environment variable is not set.");

        _http = httpClientFactory.CreateClient("badminton");
        _http.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <summary>Performs a GET request and returns the response body as a JSON string.</summary>
    public async Task<string> GetAsync(string path, string? bearerToken = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        if (!string.IsNullOrWhiteSpace(bearerToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await _http.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"GET {path} failed with {(int)response.StatusCode}: {body}");

        return body;
    }

    /// <summary>Performs a POST request with a JSON body and returns the response body as a JSON string.</summary>
    public async Task<string> PostAsync(string path, object payload, string? bearerToken = null)
    {
        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = content };
        if (!string.IsNullOrWhiteSpace(bearerToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await _http.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"POST {path} failed with {(int)response.StatusCode}: {body}");

        return body;
    }
}
