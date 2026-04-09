using Badminton_BE.DTOs;
using Badminton_BE.Models;
using Badminton_BE.Repositories.Interfaces;
using Badminton_BE.Services;
using Badminton_BE.Services.Interfaces;
using Moq;

namespace Badminton_BE.Tests.Services;

public class PaymentServiceTests
{
    private readonly Mock<ISessionPaymentRepository> _sessionPaymentRepo = new();
    private readonly Mock<IPlayerPaymentRepository> _playerPaymentRepo = new();
    private readonly Mock<ISessionRepository> _sessionRepo = new();
    private readonly Mock<ISessionPlayerRepository> _sessionPlayerRepo = new();
    private readonly Mock<INotificationService> _notificationService = new();

    private PaymentService CreateService() => new(
        _sessionPaymentRepo.Object,
        _playerPaymentRepo.Object,
        _sessionRepo.Object,
        _sessionPlayerRepo.Object,
        _notificationService.Object);

    // ── ConfirmPlayerPaymentAsync ─────────────────────────────────────────

    [Fact]
    public async Task ConfirmPlayerPaymentAsync_WhenPaymentNotFound_ReturnsNull()
    {
        _playerPaymentRepo.Setup(r => r.GetBySessionPlayerIdAsync(1))
            .ReturnsAsync((PlayerPayment?)null);

        var result = await CreateService().ConfirmPlayerPaymentAsync(1);

        Assert.Null(result);
    }

    [Fact]
    public async Task ConfirmPlayerPaymentAsync_WhenAlreadyPaid_ReturnsDtoWithoutSavingAndWasTransitionedFalse()
    {
        var payment = new PlayerPayment { Id = 1, SessionPlayerId = 1, PaidStatus = PaymentStatus.Paid };
        _playerPaymentRepo.Setup(r => r.GetBySessionPlayerIdAsync(1)).ReturnsAsync(payment);

        var result = await CreateService().ConfirmPlayerPaymentAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Paid", result.Dto!.PaidStatus);
        Assert.False(result.WasTransitioned);
        _playerPaymentRepo.Verify(r => r.Update(It.IsAny<PlayerPayment>()), Times.Never);
        _playerPaymentRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task ConfirmPlayerPaymentAsync_WhenAlreadyPending_ReturnsDtoWithoutSavingAndWasTransitionedFalse()
    {
        var payment = new PlayerPayment { Id = 1, SessionPlayerId = 1, PaidStatus = PaymentStatus.ConfirmationPending };
        _playerPaymentRepo.Setup(r => r.GetBySessionPlayerIdAsync(1)).ReturnsAsync(payment);

        var result = await CreateService().ConfirmPlayerPaymentAsync(1);

        Assert.NotNull(result);
        Assert.Equal("ConfirmationPending", result.Dto!.PaidStatus);
        Assert.False(result.WasTransitioned);
        _playerPaymentRepo.Verify(r => r.Update(It.IsAny<PlayerPayment>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmPlayerPaymentAsync_WhenNotPaid_SetsConfirmationPendingAndSavesAndWasTransitionedTrue()
    {
        var payment = new PlayerPayment { Id = 1, SessionPlayerId = 1, AmountDue = 50m, PaidStatus = PaymentStatus.NotPaid };
        _playerPaymentRepo.Setup(r => r.GetBySessionPlayerIdAsync(1)).ReturnsAsync(payment);
        _sessionPlayerRepo.Setup(r => r.GetByIdWithIncludesAsync(1)).ReturnsAsync((SessionPlayer?)null);

        var result = await CreateService().ConfirmPlayerPaymentAsync(1);

        Assert.NotNull(result);
        Assert.Equal("ConfirmationPending", result.Dto!.PaidStatus);
        Assert.True(result.WasTransitioned);
        _playerPaymentRepo.Verify(r => r.Update(It.IsAny<PlayerPayment>()), Times.Once);
        _playerPaymentRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ConfirmPlayerPaymentAsync_WhenSessionPlayerFound_UpdatesSessionPlayerStatus()
    {
        var payment = new PlayerPayment { Id = 1, SessionPlayerId = 1, PaidStatus = PaymentStatus.NotPaid };
        var sp = new SessionPlayer { Id = 1, Status = SessionPlayerStatus.Joined };
        _playerPaymentRepo.Setup(r => r.GetBySessionPlayerIdAsync(1)).ReturnsAsync(payment);
        _sessionPlayerRepo.Setup(r => r.GetByIdWithIncludesAsync(1)).ReturnsAsync(sp);

        await CreateService().ConfirmPlayerPaymentAsync(1);

        Assert.Equal(SessionPlayerStatus.ConfirmationPending, sp.Status);
    }

    // ── ApprovePlayerPaymentAsync ─────────────────────────────────────────

    [Fact]
    public async Task ApprovePlayerPaymentAsync_WhenPaymentNotFound_ReturnsNull()
    {
        _playerPaymentRepo.Setup(r => r.GetBySessionPlayerIdAsync(1))
            .ReturnsAsync((PlayerPayment?)null);

        var result = await CreateService().ApprovePlayerPaymentAsync(1);

        Assert.Null(result);
    }

    [Fact]
    public async Task ApprovePlayerPaymentAsync_SetsAmountPaidAmountDueAndPaidStatus()
    {
        var payment = new PlayerPayment { Id = 1, SessionPlayerId = 1, AmountDue = 100m, AmountPaid = 0m, PaidStatus = PaymentStatus.ConfirmationPending };
        _playerPaymentRepo.Setup(r => r.GetBySessionPlayerIdAsync(1)).ReturnsAsync(payment);
        _sessionPlayerRepo.Setup(r => r.GetByIdWithIncludesAsync(1)).ReturnsAsync((SessionPlayer?)null);

        var result = await CreateService().ApprovePlayerPaymentAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Paid", result.PaidStatus);
        Assert.Equal(100m, result.AmountPaid);
        Assert.NotNull(result.PaidAt);
    }

    [Fact]
    public async Task ApprovePlayerPaymentAsync_SavesChanges()
    {
        var payment = new PlayerPayment { Id = 1, SessionPlayerId = 1, AmountDue = 50m, PaidStatus = PaymentStatus.ConfirmationPending };
        _playerPaymentRepo.Setup(r => r.GetBySessionPlayerIdAsync(1)).ReturnsAsync(payment);
        _sessionPlayerRepo.Setup(r => r.GetByIdWithIncludesAsync(1)).ReturnsAsync((SessionPlayer?)null);

        await CreateService().ApprovePlayerPaymentAsync(1);

        _playerPaymentRepo.Verify(r => r.Update(It.IsAny<PlayerPayment>()), Times.Once);
        _playerPaymentRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ApprovePlayerPaymentAsync_WhenSessionPlayerFound_SetsStatusPaid()
    {
        var payment = new PlayerPayment { Id = 1, SessionPlayerId = 1, AmountDue = 100m, PaidStatus = PaymentStatus.ConfirmationPending };
        var sp = new SessionPlayer { Id = 1, Status = SessionPlayerStatus.ConfirmationPending };
        _playerPaymentRepo.Setup(r => r.GetBySessionPlayerIdAsync(1)).ReturnsAsync(payment);
        _sessionPlayerRepo.Setup(r => r.GetByIdWithIncludesAsync(1)).ReturnsAsync(sp);

        await CreateService().ApprovePlayerPaymentAsync(1);

        Assert.Equal(SessionPlayerStatus.Paid, sp.Status);
    }
}
