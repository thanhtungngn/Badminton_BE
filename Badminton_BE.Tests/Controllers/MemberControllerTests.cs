using Badminton_BE.Controllers;
using Badminton_BE.DTOs;
using Badminton_BE.Models;
using Badminton_BE.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Badminton_BE.Tests.Controllers;

public class MemberControllerTests
{
    private readonly Mock<IMemberService> _serviceMock = new();

    private MemberController CreateController() => new(_serviceMock.Object);

    private static MemberReadDto MakeReadDto(int id = 1) =>
        new() { Id = id, Name = "Alice", Gender = Gender.Female, Level = MemberLevel.Newbie, JoinDate = DateTime.UtcNow };

    // ── CreateMember ───────────────────────────────────────────────────

    [Fact]
    public async Task CreateMember_Returns201WithDto()
    {
        var createDto = new MemberCreateDto { Name = "Alice", Gender = Gender.Female, JoinDate = DateTime.UtcNow };
        var read = MakeReadDto();
        _serviceMock.Setup(s => s.CreateMemberAsync(createDto)).ReturnsAsync(read);

        var result = await CreateController().CreateMember(createDto);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(read, created.Value);
    }

    // ── GetMembers ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetMembers_ReturnsOkWithList()
    {
        var members = new List<MemberReadDto> { MakeReadDto(1), MakeReadDto(2) };
        _serviceMock.Setup(s => s.GetMembersAsync()).ReturnsAsync(members);

        var result = await CreateController().GetMembers();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(members, ok.Value);
    }

    // ── GetMemberByContact ─────────────────────────────────────────────

    [Fact]
    public async Task GetMemberByContact_WhenFound_ReturnsOkWithDto()
    {
        var read = MakeReadDto();
        _serviceMock.Setup(s => s.GetMemberByContactValueAsync("0900000001")).ReturnsAsync(read);

        var result = await CreateController().GetMemberByContact("0900000001");

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(read, ok.Value);
    }

    [Fact]
    public async Task GetMemberByContact_WhenNotFound_Returns404()
    {
        _serviceMock.Setup(s => s.GetMemberByContactValueAsync("0000000000")).ReturnsAsync((MemberReadDto?)null);

        var result = await CreateController().GetMemberByContact("0000000000");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetMemberByContact_WhenContactValueEmpty_Returns400()
    {
        var result = await CreateController().GetMemberByContact("   ");

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ── LookupMember ───────────────────────────────────────────────────

    [Fact]
    public async Task LookupMember_WhenFound_ReturnsOkWithDto()
    {
        var lookup = new MemberLookupDto { Name = "Alice" };
        _serviceMock.Setup(s => s.GetMemberLookupByContactAsync("0900000001")).ReturnsAsync(lookup);

        var result = await CreateController().LookupMember("0900000001");

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(lookup, ok.Value);
    }

    [Fact]
    public async Task LookupMember_WhenNotFound_Returns404()
    {
        _serviceMock.Setup(s => s.GetMemberLookupByContactAsync("0000000000")).ReturnsAsync((MemberLookupDto?)null);

        var result = await CreateController().LookupMember("0000000000");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task LookupMember_WhenContactValueEmpty_Returns400()
    {
        var result = await CreateController().LookupMember("");

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ── GetMemberById ──────────────────────────────────────────────────

    [Fact]
    public async Task GetMemberById_WhenFound_ReturnsOkWithDto()
    {
        var read = MakeReadDto();
        _serviceMock.Setup(s => s.GetMemberByIdAsync(1)).ReturnsAsync(read);

        var result = await CreateController().GetMemberById(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(read, ok.Value);
    }

    [Fact]
    public async Task GetMemberById_WhenNotFound_Returns404()
    {
        _serviceMock.Setup(s => s.GetMemberByIdAsync(99)).ReturnsAsync((MemberReadDto?)null);

        var result = await CreateController().GetMemberById(99);

        Assert.IsType<NotFoundResult>(result);
    }

    // ── UpdateMember ───────────────────────────────────────────────────

    [Fact]
    public async Task UpdateMember_WhenUpdated_Returns204()
    {
        var dto = new MemberUpdateDto { Name = "Alice", Gender = Gender.Female, JoinDate = DateTime.UtcNow };
        _serviceMock.Setup(s => s.UpdateMemberAsync(1, dto)).ReturnsAsync(true);

        var result = await CreateController().UpdateMember(1, dto);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateMember_WhenNotFound_Returns404()
    {
        var dto = new MemberUpdateDto { Name = "Alice", Gender = Gender.Female, JoinDate = DateTime.UtcNow };
        _serviceMock.Setup(s => s.UpdateMemberAsync(99, dto)).ReturnsAsync(false);

        var result = await CreateController().UpdateMember(99, dto);

        Assert.IsType<NotFoundResult>(result);
    }

    // ── DeleteMember ───────────────────────────────────────────────────

    [Fact]
    public async Task DeleteMember_WhenDeleted_Returns204()
    {
        _serviceMock.Setup(s => s.DeleteMemberAsync(1)).ReturnsAsync(true);

        var result = await CreateController().DeleteMember(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteMember_WhenNotFound_Returns404()
    {
        _serviceMock.Setup(s => s.DeleteMemberAsync(99)).ReturnsAsync(false);

        var result = await CreateController().DeleteMember(99);

        Assert.IsType<NotFoundResult>(result);
    }
}
