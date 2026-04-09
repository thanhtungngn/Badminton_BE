using Badminton_BE.Controllers;
using Badminton_BE.DTOs;
using Badminton_BE.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Badminton_BE.Tests.Controllers;

public class SessionControllerTests
{
    private readonly Mock<ISessionService> _serviceMock = new();

    private SessionController CreateController() => new(_serviceMock.Object);

    private static SessionReadDto MakeReadDto(int id = 1) =>
        new() { Id = id, Title = "Sunday", Address = "HHT", StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(2) };

    // ── CreateSession ──────────────────────────────────────────────────

    [Fact]
    public async Task CreateSession_Returns201WithDto()
    {
        var dto = new SessionCreateDto { Title = "Sunday", Address = "HHT", StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(2), PriceMale = 50000, PriceFemale = 40000, NumberOfCourts = 2 };
        var read = MakeReadDto();
        _serviceMock.Setup(s => s.CreateSessionAsync(dto)).ReturnsAsync(read);

        var result = await CreateController().CreateSession(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(read, created.Value);
    }

    // ── GetSessions ────────────────────────────────────────────────────

    [Fact]
    public async Task GetSessions_ReturnsOkWithList()
    {
        var sessions = new List<SessionReadDto> { MakeReadDto(1), MakeReadDto(2) };
        _serviceMock.Setup(s => s.GetSessionsAsync()).ReturnsAsync(sessions);

        var result = await CreateController().GetSessions();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(sessions, ok.Value);
    }

    // ── GetActiveSessions ──────────────────────────────────────────────

    [Fact]
    public async Task GetActiveSessions_ReturnsOkWithList()
    {
        var sessions = new List<SessionReadDto> { MakeReadDto(1) };
        _serviceMock.Setup(s => s.GetActiveSessionsAsync()).ReturnsAsync(sessions);

        var result = await CreateController().GetActiveSessions();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(sessions, ok.Value);
    }

    // ── GetSessionById ─────────────────────────────────────────────────

    [Fact]
    public async Task GetSessionById_WhenFound_ReturnsOkWithDto()
    {
        var read = MakeReadDto();
        _serviceMock.Setup(s => s.GetSessionByIdAsync(1)).ReturnsAsync(read);

        var result = await CreateController().GetSessionById(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(read, ok.Value);
    }

    [Fact]
    public async Task GetSessionById_WhenNotFound_Returns404()
    {
        _serviceMock.Setup(s => s.GetSessionByIdAsync(99)).ReturnsAsync((SessionReadDto?)null);

        var result = await CreateController().GetSessionById(99);

        Assert.IsType<NotFoundResult>(result);
    }

    // ── GetSessionDetail ───────────────────────────────────────────────

    [Fact]
    public async Task GetSessionDetail_WhenFound_ReturnsOkWithDto()
    {
        var detail = new SessionWithPlayersDto { Id = "1", Address = "HHT" };
        _serviceMock.Setup(s => s.GetSessionDetailAsync(1)).ReturnsAsync(detail);

        var result = await CreateController().GetSessionDetail(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(detail, ok.Value);
    }

    [Fact]
    public async Task GetSessionDetail_WhenNotFound_Returns404()
    {
        _serviceMock.Setup(s => s.GetSessionDetailAsync(99)).ReturnsAsync((SessionWithPlayersDto?)null);

        var result = await CreateController().GetSessionDetail(99);

        Assert.IsType<NotFoundResult>(result);
    }

    // ── RegisterPublic ─────────────────────────────────────────────────

    [Fact]
    public async Task RegisterPublic_WhenRegistered_Returns201()
    {
        var dto = new PublicSessionRegistrationDto { Name = "Alice", Gender = Models.Gender.Female, PhoneNumber = "0900000001" };
        var resultDto = new PublicSessionRegistrationResultDto { RegistrationStatus = PublicSessionRegistrationStatus.Registered, SessionId = 1 };
        _serviceMock.Setup(s => s.RegisterPublicAsync(1, dto)).ReturnsAsync(resultDto);

        var result = await CreateController().RegisterPublic(1, dto);

        Assert.Equal(201, ((ObjectResult)result).StatusCode);
    }

    [Fact]
    public async Task RegisterPublic_WhenSessionNotFound_Returns404()
    {
        var dto = new PublicSessionRegistrationDto { Name = "Alice", Gender = Models.Gender.Female, PhoneNumber = "0900000001" };
        var resultDto = new PublicSessionRegistrationResultDto { RegistrationStatus = PublicSessionRegistrationStatus.SessionNotFound, Message = "Not found" };
        _serviceMock.Setup(s => s.RegisterPublicAsync(99, dto)).ReturnsAsync(resultDto);

        var result = await CreateController().RegisterPublic(99, dto);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task RegisterPublic_WhenAlreadyRegistered_Returns409()
    {
        var dto = new PublicSessionRegistrationDto { Name = "Alice", Gender = Models.Gender.Female, PhoneNumber = "0900000001" };
        var resultDto = new PublicSessionRegistrationResultDto { RegistrationStatus = PublicSessionRegistrationStatus.AlreadyRegistered, Message = "Duplicate" };
        _serviceMock.Setup(s => s.RegisterPublicAsync(1, dto)).ReturnsAsync(resultDto);

        var result = await CreateController().RegisterPublic(1, dto);

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task RegisterPublic_WhenOverlappingSession_Returns409()
    {
        var dto = new PublicSessionRegistrationDto { Name = "Alice", Gender = Models.Gender.Female, PhoneNumber = "0900000001" };
        var resultDto = new PublicSessionRegistrationResultDto { RegistrationStatus = PublicSessionRegistrationStatus.OverlappingSession, Message = "Overlap" };
        _serviceMock.Setup(s => s.RegisterPublicAsync(1, dto)).ReturnsAsync(resultDto);

        var result = await CreateController().RegisterPublic(1, dto);

        Assert.IsType<ConflictObjectResult>(result);
    }

    // ── UpdateSession ──────────────────────────────────────────────────

    [Fact]
    public async Task UpdateSession_WhenUpdated_Returns204()
    {
        var dto = new SessionUpdateDto { Title = "Monday", Address = "HHT" };
        _serviceMock.Setup(s => s.UpdateSessionAsync(1, dto)).ReturnsAsync(true);

        var result = await CreateController().UpdateSession(1, dto);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateSession_WhenNotFound_Returns404()
    {
        var dto = new SessionUpdateDto { Title = "Monday", Address = "HHT" };
        _serviceMock.Setup(s => s.UpdateSessionAsync(99, dto)).ReturnsAsync(false);

        var result = await CreateController().UpdateSession(99, dto);

        Assert.IsType<NotFoundResult>(result);
    }

    // ── DeleteSession ──────────────────────────────────────────────────

    [Fact]
    public async Task DeleteSession_WhenDeleted_Returns204()
    {
        _serviceMock.Setup(s => s.DeleteSessionAsync(1)).ReturnsAsync(true);

        var result = await CreateController().DeleteSession(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteSession_WhenNotFound_Returns404()
    {
        _serviceMock.Setup(s => s.DeleteSessionAsync(99)).ReturnsAsync(false);

        var result = await CreateController().DeleteSession(99);

        Assert.IsType<NotFoundResult>(result);
    }
}
