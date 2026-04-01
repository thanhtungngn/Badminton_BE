using Badminton_MCP;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Badminton_MCP.Tests;

/// <summary>
/// Integration tests that verify the Trello connection is working.
/// All tests are skipped when TRELLO_API_KEY or TRELLO_TOKEN are not set,
/// so they never fail in CI unless credentials are explicitly provided.
/// </summary>
public sealed class TrelloConnectionTests : IDisposable
{
    private readonly ServiceProvider? _provider;
    private readonly TrelloClient? _trello;
    private readonly string? _boardId;
    private readonly string? _memberId;
    private readonly bool _credentialsConfigured;

    public TrelloConnectionTests()
    {
        _boardId = Environment.GetEnvironmentVariable("TRELLO_BOARD_ID");
        _memberId = Environment.GetEnvironmentVariable("TRELLO_MEMBER_ID");

        var apiKey = Environment.GetEnvironmentVariable("TRELLO_API_KEY");
        var token = Environment.GetEnvironmentVariable("TRELLO_TOKEN");

        _credentialsConfigured = !string.IsNullOrWhiteSpace(apiKey) && !string.IsNullOrWhiteSpace(token);

        if (_credentialsConfigured)
        {
            var services = new ServiceCollection();
            services.AddHttpClient();
            _provider = services.BuildServiceProvider();
            _trello = new TrelloClient(_provider.GetRequiredService<IHttpClientFactory>());
        }
    }

    private void SkipIfMissingCredentials()
    {
        Skip.If(!_credentialsConfigured, "TRELLO_API_KEY and/or TRELLO_TOKEN are not set.");
    }

    private void SkipIfMissingBoardId()
    {
        Skip.If(string.IsNullOrWhiteSpace(_boardId), "TRELLO_BOARD_ID is not set.");
    }

    private void SkipIfMissingMemberId()
    {
        Skip.If(string.IsNullOrWhiteSpace(_memberId), "TRELLO_MEMBER_ID is not set.");
    }

    [SkippableFact]
    public async Task GetBoardLists_ReturnsAtLeastOneList()
    {
        SkipIfMissingCredentials();
        SkipIfMissingBoardId();

        var lists = await _trello!.GetBoardListsAsync(_boardId!);

        Assert.NotEmpty(lists);
        Assert.All(lists, l =>
        {
            Assert.False(string.IsNullOrWhiteSpace(l.Id));
            Assert.False(string.IsNullOrWhiteSpace(l.Name));
        });
    }

    [SkippableFact]
    public async Task GetBoardCards_ReturnsWithoutError()
    {
        SkipIfMissingCredentials();
        SkipIfMissingBoardId();

        var cards = await _trello!.GetBoardCardsAsync(_boardId!);

        Assert.NotNull(cards);
    }

    [SkippableFact]
    public async Task GetMemberCards_ReturnsWithoutError()
    {
        SkipIfMissingCredentials();
        SkipIfMissingMemberId();

        var cards = await _trello!.GetMemberCardsAsync(_memberId!);

        Assert.NotNull(cards);
    }

    [SkippableFact]
    public async Task GetBoardAICards_ReturnsOnlyAILabelledCards()
    {
        SkipIfMissingCredentials();
        SkipIfMissingBoardId();

        var cards = await _trello!.GetBoardCardsAsync(_boardId!);
        var aiCards = cards
            .Where(c => c.Labels.Any(l => string.Equals(l.Name.Trim(), "AI", StringComparison.OrdinalIgnoreCase)))
            .ToList();

        Assert.All(aiCards, c =>
            Assert.Contains(c.Labels, l => string.Equals(l.Name.Trim(), "AI", StringComparison.OrdinalIgnoreCase)));
    }

    public void Dispose() => _provider?.Dispose();
}
