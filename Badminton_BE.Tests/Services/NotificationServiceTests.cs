using Badminton_BE.Models;
using Badminton_BE.Repositories.Interfaces;
using Badminton_BE.Services;
using Badminton_BE.Tests.Helpers;
using Moq;

namespace Badminton_BE.Tests.Services;

public class NotificationServiceTests
{
    private readonly Mock<INotificationRepository> _repoMock = new();

    private NotificationService CreateService(TestCurrentUserService? user = null, Data.AppDbContext? db = null)
    {
        var userService = user ?? new TestCurrentUserService();
        return new NotificationService(_repoMock.Object, userService, db ?? DbContextFactory.Create(userService));
    }

    private static Session MakeSession(int id = 1) =>
        new() { Id = id, UserId = 1, Title = "Sunday", Address = "HHT", StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(2) };

    private static Member MakeMember(int id = 1) =>
        new() { Id = id, UserId = 1, Name = "Alice", Gender = Gender.Female, Level = MemberLevel.Newbie, JoinDate = DateTime.UtcNow };

    // ── TriggerPriceChangedAsync ──────────────────────────────────────────

    [Fact]
    public async Task TriggerPriceChangedAsync_WhenNotAuthenticated_DoesNothing()
    {
        await CreateService(TestCurrentUserService.Unauthenticated).TriggerPriceChangedAsync(1, 50000, 40000);

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Never);
    }

    [Fact]
    public async Task TriggerPriceChangedAsync_WhenSessionNotFound_DoesNothing()
    {
        await CreateService().TriggerPriceChangedAsync(999, 50000, 40000);

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Never);
    }

    [Fact]
    public async Task TriggerPriceChangedAsync_WhenSessionFound_CreatesNotificationAndSaves()
    {
        var user = new TestCurrentUserService();
        var db = DbContextFactory.Create(user);
        db.Sessions.Add(MakeSession());
        await db.SaveChangesAsync();

        await CreateService(user, db).TriggerPriceChangedAsync(1, 50000, 40000);

        _repoMock.Verify(r => r.AddAsync(It.Is<Notification>(n =>
            n.Type == NotificationType.PriceChanged && n.SessionId == 1 && n.UserId == 1)), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // ── TriggerPaymentRecordedAsync ───────────────────────────────────────

    [Fact]
    public async Task TriggerPaymentRecordedAsync_WhenNotAuthenticated_DoesNothing()
    {
        await CreateService(TestCurrentUserService.Unauthenticated).TriggerPaymentRecordedAsync(1);

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Never);
    }

    [Fact]
    public async Task TriggerPaymentRecordedAsync_WhenSessionPlayerNotFound_DoesNothing()
    {
        await CreateService().TriggerPaymentRecordedAsync(999);

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Never);
    }

    [Fact]
    public async Task TriggerPaymentRecordedAsync_WhenFound_CreatesPaymentRecordedNotification()
    {
        var user = new TestCurrentUserService();
        var db = DbContextFactory.Create(user);
        db.Sessions.Add(MakeSession());
        db.Members.Add(MakeMember());
        await db.SaveChangesAsync();
        db.SessionPlayers.Add(new SessionPlayer { Id = 1, UserId = 1, SessionId = 1, MemberId = 1 });
        await db.SaveChangesAsync();

        await CreateService(user, db).TriggerPaymentRecordedAsync(1);

        _repoMock.Verify(r => r.AddAsync(It.Is<Notification>(n =>
            n.Type == NotificationType.PaymentRecorded && n.SessionId == 1)), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // ── TriggerUnpaidReminderAsync ────────────────────────────────────────

    [Fact]
    public async Task TriggerUnpaidReminderAsync_WhenNotAuthenticated_DoesNothing()
    {
        await CreateService(TestCurrentUserService.Unauthenticated).TriggerUnpaidReminderAsync(1);

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Never);
    }

    [Fact]
    public async Task TriggerUnpaidReminderAsync_WhenAlreadyRemindedToday_Skips()
    {
        _repoMock.Setup(r => r.ExistsTodayAsync(1, NotificationType.UnpaidReminder)).ReturnsAsync(true);

        await CreateService().TriggerUnpaidReminderAsync(1);

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Never);
    }

    [Fact]
    public async Task TriggerUnpaidReminderAsync_WhenSessionNotFound_DoesNothing()
    {
        _repoMock.Setup(r => r.ExistsTodayAsync(999, NotificationType.UnpaidReminder)).ReturnsAsync(false);

        await CreateService().TriggerUnpaidReminderAsync(999);

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Never);
    }

    [Fact]
    public async Task TriggerUnpaidReminderAsync_WhenNoUnpaidPlayers_DoesNothing()
    {
        _repoMock.Setup(r => r.ExistsTodayAsync(1, NotificationType.UnpaidReminder)).ReturnsAsync(false);
        var user = new TestCurrentUserService();
        var db = DbContextFactory.Create(user);
        db.Sessions.Add(MakeSession());
        await db.SaveChangesAsync();

        await CreateService(user, db).TriggerUnpaidReminderAsync(1);

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Never);
    }

    [Fact]
    public async Task TriggerUnpaidReminderAsync_WhenUnpaidPlayersExist_CreatesNotification()
    {
        _repoMock.Setup(r => r.ExistsTodayAsync(1, NotificationType.UnpaidReminder)).ReturnsAsync(false);
        var user = new TestCurrentUserService();
        var db = DbContextFactory.Create(user);
        db.Sessions.Add(MakeSession());
        db.Members.Add(MakeMember());
        await db.SaveChangesAsync();
        db.SessionPlayers.Add(new SessionPlayer { Id = 1, UserId = 1, SessionId = 1, MemberId = 1 });
        await db.SaveChangesAsync();
        db.PlayerPayments.Add(new PlayerPayment { Id = 1, UserId = 1, SessionPlayerId = 1, AmountDue = 50m, PaidStatus = PaymentStatus.NotPaid });
        await db.SaveChangesAsync();

        await CreateService(user, db).TriggerUnpaidReminderAsync(1);

        _repoMock.Verify(r => r.AddAsync(It.Is<Notification>(n =>
            n.Type == NotificationType.UnpaidReminder && n.SessionId == 1)), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
