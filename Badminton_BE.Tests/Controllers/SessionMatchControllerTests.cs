using Badminton_BE.Controllers;
using Badminton_BE.DTOs;
using Badminton_BE.Models;
using Badminton_BE.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Badminton_BE.Tests.Controllers;

public class SessionMatchControllerTests
{
    private readonly Mock<ISessionMatchService> _serviceMock = new();

    private SessionMatchController CreateController() => new(_serviceMock.Object);

    private static SessionMatchReadDto MakeReadDto(int id = 1) =>
        new() { Id = id, SessionId = 1, TeamAScore = 21, TeamBScore = 15, Winner = MatchWinner.TeamA };

    private static SessionMatchUpsertDto MakeUpsertDto() =>
        new() { TeamAPlayerIds = [1], TeamBPlayerIds = [2], TeamAScore = 21, TeamBScore = 15, Winner = MatchWinner.TeamA };

    // ── GetMatches ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetMatches_ReturnsOkWithList()
    {
        var matches = new List<SessionMatchReadDto> { MakeReadDto(1), MakeReadDto(2) };
        _serviceMock.Setup(s => s.GetBySessionIdAsync(1)).ReturnsAsync(matches);

        var result = await CreateController().GetMatches(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(matches, ok.Value);
    }

    // ── GetMatch ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetMatch_WhenFound_ReturnsOkWithDto()
    {
        var match = MakeReadDto();
        _serviceMock.Setup(s => s.GetByIdAsync(1, 1)).ReturnsAsync(match);

        var result = await CreateController().GetMatch(1, 1);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(match, ok.Value);
    }

    [Fact]
    public async Task GetMatch_WhenNotFound_Returns404()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1, 99)).ReturnsAsync((SessionMatchReadDto?)null);

        var result = await CreateController().GetMatch(1, 99);

        Assert.IsType<NotFoundResult>(result);
    }

    // ── CreateMatch ────────────────────────────────────────────────────

    [Fact]
    public async Task CreateMatch_WhenCreated_Returns201WithDto()
    {
        var dto = MakeUpsertDto();
        var match = MakeReadDto();
        _serviceMock.Setup(s => s.CreateAsync(1, dto)).ReturnsAsync(match);

        var result = await CreateController().CreateMatch(1, dto);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(match, created.Value);
    }

    [Fact]
    public async Task CreateMatch_WhenServiceReturnsNull_Returns400()
    {
        var dto = MakeUpsertDto();
        _serviceMock.Setup(s => s.CreateAsync(1, dto)).ReturnsAsync((SessionMatchReadDto?)null);

        var result = await CreateController().CreateMatch(1, dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ── UpdateMatch ────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateMatch_WhenUpdated_ReturnsOkWithDto()
    {
        var dto = MakeUpsertDto();
        var existing = MakeReadDto();
        var updated = MakeReadDto();
        _serviceMock.Setup(s => s.GetByIdAsync(1, 1)).ReturnsAsync(existing);
        _serviceMock.Setup(s => s.UpdateAsync(1, 1, dto)).ReturnsAsync(updated);

        var result = await CreateController().UpdateMatch(1, 1, dto);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(updated, ok.Value);
    }

    [Fact]
    public async Task UpdateMatch_WhenMatchNotFound_Returns404()
    {
        var dto = MakeUpsertDto();
        _serviceMock.Setup(s => s.GetByIdAsync(1, 99)).ReturnsAsync((SessionMatchReadDto?)null);

        var result = await CreateController().UpdateMatch(1, 99, dto);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateMatch_WhenUpdateFails_Returns400()
    {
        var dto = MakeUpsertDto();
        var existing = MakeReadDto();
        _serviceMock.Setup(s => s.GetByIdAsync(1, 1)).ReturnsAsync(existing);
        _serviceMock.Setup(s => s.UpdateAsync(1, 1, dto)).ReturnsAsync((SessionMatchReadDto?)null);

        var result = await CreateController().UpdateMatch(1, 1, dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ── DeleteMatch ────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteMatch_WhenDeleted_Returns204()
    {
        _serviceMock.Setup(s => s.DeleteAsync(1, 1)).ReturnsAsync(true);

        var result = await CreateController().DeleteMatch(1, 1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteMatch_WhenNotFound_Returns404()
    {
        _serviceMock.Setup(s => s.DeleteAsync(1, 99)).ReturnsAsync(false);

        var result = await CreateController().DeleteMatch(1, 99);

        Assert.IsType<NotFoundResult>(result);
    }
}
