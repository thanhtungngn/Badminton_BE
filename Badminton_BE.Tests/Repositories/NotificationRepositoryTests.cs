using Badminton_BE.Models;
using Badminton_BE.Repositories;
using Badminton_BE.Tests.Helpers;

namespace Badminton_BE.Tests.Repositories;

public class NotificationRepositoryTests
{
    private static (NotificationRepository repo, Data.AppDbContext db) Create()
    {
        var user = new TestCurrentUserService();
        var db = DbContextFactory.Create(user);
        return (new NotificationRepository(db), db);
    }

    private static Notification MakeNotification(int id, bool isRead = false, int? sessionId = null) =>
        new() { Id = id, UserId = 1, Type = NotificationType.PaymentRecorded, IsRead = isRead, Payload = "{}", SessionId = sessionId };

    // ── GetPagedAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetPagedAsync_ReturnsCorrectItemsAndTotal()
    {
        var (repo, db) = Create();
        db.Notifications.AddRange(MakeNotification(1), MakeNotification(2), MakeNotification(3));
        await db.SaveChangesAsync();

        var (items, total) = await repo.GetPagedAsync(page: 1, pageSize: 2);

        Assert.Equal(3, total);
        Assert.Equal(2, items.Count());
    }

    [Fact]
    public async Task GetPagedAsync_SecondPage_ReturnsRemainingItems()
    {
        var (repo, db) = Create();
        db.Notifications.AddRange(MakeNotification(1), MakeNotification(2), MakeNotification(3));
        await db.SaveChangesAsync();

        var (items, total) = await repo.GetPagedAsync(page: 2, pageSize: 2);

        Assert.Equal(3, total);
        Assert.Single(items);
    }

    // ── GetUnreadCountAsync ───────────────────────────────────────────────

    [Fact]
    public async Task GetUnreadCountAsync_CountsOnlyUnread()
    {
        var (repo, db) = Create();
        db.Notifications.AddRange(MakeNotification(1, isRead: false), MakeNotification(2, isRead: true), MakeNotification(3, isRead: false));
        await db.SaveChangesAsync();

        var count = await repo.GetUnreadCountAsync();

        Assert.Equal(2, count);
    }

    // ── GetByIdForCurrentUserAsync ────────────────────────────────────────

    [Fact]
    public async Task GetByIdForCurrentUserAsync_WhenExists_ReturnsNotification()
    {
        var (repo, db) = Create();
        db.Notifications.Add(MakeNotification(1));
        await db.SaveChangesAsync();

        var result = await repo.GetByIdForCurrentUserAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetByIdForCurrentUserAsync_WhenNotFound_ReturnsNull()
    {
        var (repo, _) = Create();

        var result = await repo.GetByIdForCurrentUserAsync(999);

        Assert.Null(result);
    }

    // ── MarkAsReadAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task MarkAsReadAsync_SetsIsReadTrue()
    {
        var (repo, db) = Create();
        db.Notifications.Add(MakeNotification(1, isRead: false));
        await db.SaveChangesAsync();

        await repo.MarkAsReadAsync(1);

        var updated = db.Notifications.Find(1);
        Assert.True(updated?.IsRead);
    }

    [Fact]
    public async Task MarkAsReadAsync_WhenAlreadyRead_DoesNotThrow()
    {
        var (repo, db) = Create();
        db.Notifications.Add(MakeNotification(1, isRead: true));
        await db.SaveChangesAsync();

        var ex = await Record.ExceptionAsync(() => repo.MarkAsReadAsync(1));

        Assert.Null(ex);
    }

    // ── MarkAllAsReadAsync ────────────────────────────────────────────────

    [Fact]
    public async Task MarkAllAsReadAsync_SetsAllUnreadToRead()
    {
        var (repo, db) = Create();
        db.Notifications.AddRange(MakeNotification(1, isRead: false), MakeNotification(2, isRead: false), MakeNotification(3, isRead: true));
        await db.SaveChangesAsync();

        await repo.MarkAllAsReadAsync();

        Assert.True(db.Notifications.All(n => n.IsRead));
    }

    // ── ExistsTodayAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task ExistsTodayAsync_WhenCreatedToday_ReturnsTrue()
    {
        var (repo, db) = Create();
        db.Notifications.Add(new Notification { Id = 1, UserId = 1, Type = NotificationType.UnpaidReminder, SessionId = 1, Payload = "{}", CreatedDate = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var result = await repo.ExistsTodayAsync(1, NotificationType.UnpaidReminder);

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsTodayAsync_WhenNoMatchingEntry_ReturnsFalse()
    {
        var (repo, _) = Create();

        var result = await repo.ExistsTodayAsync(1, NotificationType.UnpaidReminder);

        Assert.False(result);
    }
}
