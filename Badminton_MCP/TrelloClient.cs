using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Badminton_MCP;

/// <summary>
/// Lightweight HTTP client for the Trello REST API v1.
/// Credentials are read from TRELLO_API_KEY and TRELLO_TOKEN environment variables.
/// </summary>
public sealed class TrelloClient
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _token;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public TrelloClient(IHttpClientFactory httpClientFactory)
    {
        _apiKey = Environment.GetEnvironmentVariable("TRELLO_API_KEY")
            ?? throw new InvalidOperationException("TRELLO_API_KEY environment variable is not set.");
        _token = Environment.GetEnvironmentVariable("TRELLO_TOKEN")
            ?? throw new InvalidOperationException("TRELLO_TOKEN environment variable is not set.");

        _http = httpClientFactory.CreateClient("trello");
        _http.BaseAddress = new Uri("https://api.trello.com/1/");
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    private string AppendAuth(string path)
    {
        var sep = path.Contains('?') ? "&" : "?";
        return $"{path}{sep}key={_apiKey}&token={_token}";
    }

    /// <summary>Returns all open cards assigned to the given member ID.</summary>
    public async Task<List<TrelloCard>> GetMemberCardsAsync(string memberId)
    {
        var url = AppendAuth($"members/{Uri.EscapeDataString(memberId)}/cards?filter=open&fields=id,name,desc,labels,shortUrl,idList,idBoard");
        var json = await GetRawAsync(url);
        return JsonSerializer.Deserialize<List<TrelloCard>>(json, JsonOptions) ?? [];
    }

    /// <summary>Returns all cards on a given board.</summary>
    public async Task<List<TrelloCard>> GetBoardCardsAsync(string boardId)
    {
        var url = AppendAuth($"boards/{Uri.EscapeDataString(boardId)}/cards?filter=open&fields=id,name,desc,labels,shortUrl,idList,idMembers");
        var json = await GetRawAsync(url);
        return JsonSerializer.Deserialize<List<TrelloCard>>(json, JsonOptions) ?? [];
    }

    /// <summary>Returns all lists on a board.</summary>
    public async Task<List<TrelloList>> GetBoardListsAsync(string boardId)
    {
        var url = AppendAuth($"boards/{Uri.EscapeDataString(boardId)}/lists?fields=id,name");
        var json = await GetRawAsync(url);
        return JsonSerializer.Deserialize<List<TrelloList>>(json, JsonOptions) ?? [];
    }

    /// <summary>Moves a card to the given list.</summary>
    public async Task MoveCardToListAsync(string cardId, string listId)
    {
        var url = AppendAuth($"cards/{Uri.EscapeDataString(cardId)}");
        using var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("idList", listId) });
        using var request = new HttpRequestMessage(new HttpMethod("PUT"), url) { Content = content };
        using var response = await _http.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"PUT card {cardId} failed with {(int)response.StatusCode}: {body}");
        }
    }

    /// <summary>Adds a comment to a Trello card.</summary>
    public async Task AddCommentAsync(string cardId, string text)
    {
        var url = AppendAuth($"cards/{Uri.EscapeDataString(cardId)}/actions/comments");
        using var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("text", text) });
        using var response = await _http.PostAsync(url, content);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"POST comment on card {cardId} failed with {(int)response.StatusCode}: {body}");
        }
    }

    private async Task<string> GetRawAsync(string url)
    {
        using var response = await _http.GetAsync(url);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"GET {url} failed with {(int)response.StatusCode}: {body}");
        return body;
    }
}

public sealed class TrelloCard
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("desc")]
    public string Desc { get; set; } = string.Empty;

    [JsonPropertyName("shortUrl")]
    public string ShortUrl { get; set; } = string.Empty;

    [JsonPropertyName("idList")]
    public string IdList { get; set; } = string.Empty;

    [JsonPropertyName("idBoard")]
    public string IdBoard { get; set; } = string.Empty;

    [JsonPropertyName("idMembers")]
    public List<string> IdMembers { get; set; } = [];

    [JsonPropertyName("labels")]
    public List<TrelloLabel> Labels { get; set; } = [];
}

public sealed class TrelloLabel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("color")]
    public string Color { get; set; } = string.Empty;
}

public sealed class TrelloList
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
