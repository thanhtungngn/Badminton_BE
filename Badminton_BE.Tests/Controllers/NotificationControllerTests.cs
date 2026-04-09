using Badminton_BE.Controllers;
using Badminton_BE.DTOs;
using Badminton_BE.Models;
using Badminton_BE.Repositories.Interfaces;
using Badminton_BE.Services.Interfaces;
using Badminton_BE.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Badminton_BE.Tests.Controllers;

public class NotificationControllerTests
{
    private readonly Mock<INotificationRepository> _repoMock = new();
    private readonly Mock<INotificationService> _serviceMock = new();

    private NotificationController CreateController(Data.AppDbContext? db = null) =>
        new(_repoMock.Object, _serviceMock.Object, db ?? DbContextFactory.Create());

    // ── GetNotifications ──────────────────────────────────────────────────

    [Fact]
    public async Task GetNotifications_ReturnsPagedDto()
    {
        var items = new List<Notification>
        {
            new() { Id = 1, UserId = 1, Type = NotificationType.PaymentRecorded, IsRead = false, Payload = "{}" },
            new() { Id = 2, UserId = 1, Type = NotificationType.UnpaidReminder, IsRead = true, Payload = "{}" }
        };
        _repoMock.Setup(r => r.GetPagedAsync(1, 20)).ReturnsAsync((items, 2));

        var result = await CreateController().GetNotifications(page: 1, pageSize: 20);

        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<NotificationPagedDto>(ok.Value);
        Assert.Equal(2, dto.TotalCount);
        Assert.Equal(2, dto.Items.Count());
    }

    [Fact]
    public async Task GetNotifications_ClampsPageSizeToMax100()
    {
        _repoMock.Setup(r => r.GetPagedAsync(1, 20)).ReturnsAsync((new List<Notification>(), 0));

        await CreateController().GetNotifications(page: 1, pageSize: 999);

        _repoMock.Verify(r => r.GetPagedAsync(1, 20), Times.Once);
    }

    // ── GetUnreadCount ────────────────────────────────────────────────────

    [Fact]
    public async Task GetUnreadCount_ReturnsCorrectCount()
    {
        _repoMock.Setup(r => r.GetUnreadCountAsync()).ReturnsAsync(5);

        var result = await CreateController().GetUnreadCount();

        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<UnreadCountDto>(ok.Value);
        Assert.Equal(5, dto.Count);
    }

    // ── MarkAsRead ────────────────────────────────────────────────────────

    [Fact]
    public async Task MarkAsRead_WhenNotificationNotFound_Returns404()
    {
        _repoMock.Setup(r => r.GetByIdForCurrentUserAsync(1)).ReturnsAsync((Notification?)null);

        var result = await CreateController().MarkAsRead(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task MarkAsRead_WhenFound_Returns204AndMarksRead()
    {
        var notification = new Notification { Id = 1, UserId = 1, Payload = "{}" };
        _repoMock.Setup(r => r.GetByIdForCurrentUserAsync(1)).ReturnsAsync(notification);
        _repoMock.Setup(r => r.MarkAsReadAsync(1)).Returns(Task.CompletedTask);

        var result = await CreateController().MarkAsRead(1);

        Assert.IsType<NoContentResult>(result);
        _repoMock.Verify(r => r.MarkAsReadAsync(1), Times.Once);
    }

    // ── MarkAllAsRead ─────────────────────────────────────────────────────

    [Fact]
    public async Task MarkAllAsRead_Returns204()
    {
        _repoMock.Setup(r => r.MarkAllAsReadAsync()).Returns(Task.CompletedTask);

        var result = await CreateController().MarkAllAsRead();

        Assert.IsType<NoContentResult>(result);
        _repoMock.Verify(r => r.MarkAllAsReadAsync(), Times.Once);
    }

    // ── TriggerReminder ───────────────────────────────────────────────────

    [Fact]
    public async Task TriggerReminder_WithNoOngoingSessions_ReturnsZeroCounts()
    {
        var result = await CreateController().TriggerReminder();

        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<TriggerReminderResultDto>(ok.Value);
        Assert.Equal(0, dto.SessionsProcessed);
        Assert.Equal(0, dto.NotificationsCreated);
    }

    [Fact]
    public async Task TriggerReminder_WithOngoingSession_NotYetReminded_CreatesNotification()
    {
        var user = new TestCurrentUserService();
        var db = DbContextFactory.Create(user);
        db.Sessions.Add(new Session { Id = 1, UserId = 1, Title = "S", Address = "A", StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(2), Status = SessionStatus.OnGoing });
        await db.SaveChangesAsync();
        _repoMock.Setup(r => r.ExistsTodayAsync(1, NotificationType.UnpaidReminder)).ReturnsAsync(false);
        _serviceMock.Setup(s => s.TriggerUnpaidReminderAsync(1)).Returns(Task.CompletedTask);

        var result = await CreateController(db).TriggerReminder();

        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<TriggerReminderResultDto>(ok.Value);
        Assert.Equal(1, dto.NotificationsCreated);
        Assert.Equal(0, dto.Skipped);
        _serviceMock.Verify(s => s.TriggerUnpaidReminderAsync(1), Times.Once);
    }

    [Fact]
    public async Task TriggerReminder_WhenAlreadyRemindedToday_SkipsSession()
    {
        var user = new TestCurrentUserService();
        var db = DbContextFactory.Create(user);
        db.Sessions.Add(new Session { Id = 1, UserId = 1, Title = "S", Address = "A", StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(2), Status = SessionStatus.OnGoing });
        await db.SaveChangesAsync();
        _repoMock.Setup(r => r.ExistsTodayAsync(1, NotificationType.UnpaidReminder)).ReturnsAsync(true);

        var result = await CreateController(db).TriggerReminder();

        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<TriggerReminderResultDto>(ok.Value);
        Assert.Equal(1, dto.Skipped);
        Assert.Equal(0, dto.NotificationsCreated);
        _serviceMock.Verify(s => s.TriggerUnpaidReminderAsync(It.IsAny<int>()), Times.Never);
    }
}
