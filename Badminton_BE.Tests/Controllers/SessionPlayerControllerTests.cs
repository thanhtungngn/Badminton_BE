using Badminton_BE.Controllers;
using Badminton_BE.DTOs;
using Badminton_BE.Models;
using Badminton_BE.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Badminton_BE.Tests.Controllers;

public class SessionPlayerControllerTests
{
    private readonly Mock<ISessionPlayerService> _serviceMock = new();

    private SessionPlayerController CreateController() => new(_serviceMock.Object);

    private static SessionPlayerReadDto MakeReadDto(int id = 1) =>
        new() { Id = id, SessionId = 1, MemberId = 1, Status = SessionPlayerStatus.Joined };

    // ── AddMemberToSession ─────────────────────────────────────────────

    [Fact]
    public async Task AddMemberToSession_WhenCreated_Returns201WithDto()
    {
        var createDto = new SessionPlayerCreateDto { SessionId = 1, MemberId = 1 };
        var read = MakeReadDto();
        _serviceMock.Setup(s => s.AddMemberToSessionAsync(createDto)).ReturnsAsync(read);

        var result = await CreateController().AddMemberToSession(createDto);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(read, created.Value);
    }

    [Fact]
    public async Task AddMemberToSession_WhenServiceReturnsNull_Returns409()
    {
        var createDto = new SessionPlayerCreateDto { SessionId = 1, MemberId = 1 };
        _serviceMock.Setup(s => s.AddMemberToSessionAsync(createDto)).ReturnsAsync((SessionPlayerReadDto?)null);

        var result = await CreateController().AddMemberToSession(createDto);

        Assert.IsType<ConflictObjectResult>(result);
    }

    // ── UpdateStatus ───────────────────────────────────────────────────

    [Fact]
    public async Task UpdateStatus_WhenUpdated_Returns204()
    {
        _serviceMock.Setup(s => s.ChangeStatusAsync(1, SessionPlayerStatus.Paid)).ReturnsAsync(true);

        var result = await CreateController().UpdateStatus(1, new SessionPlayerStatusUpdateDto { Status = SessionPlayerStatus.Paid });

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateStatus_WhenNotFound_Returns404()
    {
        _serviceMock.Setup(s => s.ChangeStatusAsync(99, SessionPlayerStatus.Paid)).ReturnsAsync(false);

        var result = await CreateController().UpdateStatus(99, new SessionPlayerStatusUpdateDto { Status = SessionPlayerStatus.Paid });

        Assert.IsType<NotFoundResult>(result);
    }

    // ── GetSessionPlayer ───────────────────────────────────────────────

    [Fact]
    public async Task GetSessionPlayer_WhenFound_ReturnsOkWithDto()
    {
        var read = MakeReadDto();
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(read);

        var result = await CreateController().GetSessionPlayer(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(read, ok.Value);
    }

    [Fact]
    public async Task GetSessionPlayer_WhenNotFound_Returns404()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((SessionPlayerReadDto?)null);

        var result = await CreateController().GetSessionPlayer(99);

        Assert.IsType<NotFoundResult>(result);
    }

    // ── Remove ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Remove_WhenRemoved_Returns204()
    {
        _serviceMock.Setup(s => s.RemoveAsync(1)).ReturnsAsync(true);

        var result = await CreateController().Remove(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Remove_WhenNotFound_Returns404()
    {
        _serviceMock.Setup(s => s.RemoveAsync(99)).ReturnsAsync(false);

        var result = await CreateController().Remove(99);

        Assert.IsType<NotFoundResult>(result);
    }
}
