using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Badminton_MCP;

/// <summary>
/// HTTP client that wraps the Badminton REST API.
/// The API base URL is read from BADMINTON_API_URL.
/// An optional JWT token is read from BADMINTON_API_TOKEN at startup
/// and can be updated at runtime via SetToken() after a Login call.
/// </summary>
public sealed class BadmintonApiClient
{
    private readonly HttpClient _http;
    private string? _token;

    public BadmintonApiClient(IHttpClientFactory httpClientFactory)
    {
        var baseUrl = Environment.GetEnvironmentVariable("BADMINTON_API_URL")
            ?? throw new InvalidOperationException("BADMINTON_API_URL environment variable is not set.");

        _token = Environment.GetEnvironmentVariable("BADMINTON_API_TOKEN");

        _http = httpClientFactory.CreateClient("badminton");
        _http.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <summary>Stores a JWT token to be used automatically on all subsequent requests.</summary>
    public void SetToken(string token) => _token = token;

    /// <summary>Performs a GET request and returns the response body as a JSON string.</summary>
    public async Task<string> GetAsync(string path)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        ApplyToken(request);
        return await SendAsync(request, $"GET {path}");
    }

    /// <summary>Performs a POST request with a JSON body and returns the response body as a JSON string.</summary>
    public async Task<string> PostAsync(string path, object payload)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = Serialize(payload)
        };
        ApplyToken(request);
        return await SendAsync(request, $"POST {path}");
    }

    /// <summary>Performs a PUT request with a JSON body and returns the response body as a JSON string.</summary>
    public async Task<string> PutAsync(string path, object payload)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, path)
        {
            Content = Serialize(payload)
        };
        ApplyToken(request);
        return await SendAsync(request, $"PUT {path}");
    }

    /// <summary>Performs a PATCH request with a JSON body and returns the response body as a JSON string.</summary>
    public async Task<string> PatchAsync(string path, object payload)
    {
        using var request = new HttpRequestMessage(HttpMethod.Patch, path)
        {
            Content = Serialize(payload)
        };
        ApplyToken(request);
        return await SendAsync(request, $"PATCH {path}");
    }

    /// <summary>Performs a DELETE request and returns the response body (or a success message).</summary>
    public async Task<string> DeleteAsync(string path)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, path);
        ApplyToken(request);
        return await SendAsync(request, $"DELETE {path}");
    }

    // -----------------------------------------------------------------------

    private void ApplyToken(HttpRequestMessage request)
    {
        if (!string.IsNullOrWhiteSpace(_token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
    }

    private static StringContent Serialize(object payload) =>
        new(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

    private async Task<string> SendAsync(HttpRequestMessage request, string label)
    {
        using var response = await _http.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"{label} failed with {(int)response.StatusCode}: {body}");
        return body;
    }
}

