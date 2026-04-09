using Badminton_BE.Controllers;
using Badminton_BE.DTOs;
using Badminton_BE.Services;
using Badminton_BE.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Badminton_BE.Tests.Controllers;

public class PaymentControllerTests
{
    private readonly Mock<IPaymentService> _paymentService = new();
    private readonly Mock<INotificationService> _notificationService = new();

    private PaymentController CreateController() =>
        new(_paymentService.Object, _notificationService.Object);

    // ── ConfirmPlayerPayment ──────────────────────────────────────────────

    [Fact]
    public async Task ConfirmPlayerPayment_WhenPaymentNotFound_Returns404()
    {
        _paymentService.Setup(s => s.ConfirmPlayerPaymentAsync(1))
            .ReturnsAsync((PlayerPaymentReadDto?)null);

        var result = await CreateController().ConfirmPlayerPayment(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task ConfirmPlayerPayment_WhenSucceeds_Returns200WithDto()
    {
        var dto = new PlayerPaymentReadDto { Id = 1, SessionPlayerId = 1, PaidStatus = "ConfirmationPending" };
        _paymentService.Setup(s => s.ConfirmPlayerPaymentAsync(1)).ReturnsAsync(dto);
        _notificationService.Setup(s => s.TriggerPaymentRecordedAsync(1)).Returns(Task.CompletedTask);

        var result = await CreateController().ConfirmPlayerPayment(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(dto, ok.Value);
    }

    [Fact]
    public async Task ConfirmPlayerPayment_WhenSucceeds_TriggersNotification()
    {
        var dto = new PlayerPaymentReadDto { Id = 1, SessionPlayerId = 1, PaidStatus = "ConfirmationPending" };
        _paymentService.Setup(s => s.ConfirmPlayerPaymentAsync(1)).ReturnsAsync(dto);
        _notificationService.Setup(s => s.TriggerPaymentRecordedAsync(1)).Returns(Task.CompletedTask);

        await CreateController().ConfirmPlayerPayment(1);

        _notificationService.Verify(s => s.TriggerPaymentRecordedAsync(1), Times.Once);
    }

    // ── ApprovePlayerPayment ──────────────────────────────────────────────

    [Fact]
    public async Task ApprovePlayerPayment_WhenPaymentNotFound_Returns404()
    {
        _paymentService.Setup(s => s.ApprovePlayerPaymentAsync(1))
            .ReturnsAsync((PlayerPaymentReadDto?)null);

        var result = await CreateController().ApprovePlayerPayment(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task ApprovePlayerPayment_WhenSucceeds_Returns200WithDto()
    {
        var dto = new PlayerPaymentReadDto { Id = 1, SessionPlayerId = 1, PaidStatus = "Paid", AmountPaid = 100m };
        _paymentService.Setup(s => s.ApprovePlayerPaymentAsync(1)).ReturnsAsync(dto);

        var result = await CreateController().ApprovePlayerPayment(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(dto, ok.Value);
    }

    [Fact]
    public async Task ApprovePlayerPayment_DoesNotTriggerNotification()
    {
        var dto = new PlayerPaymentReadDto { Id = 1, SessionPlayerId = 1, PaidStatus = "Paid" };
        _paymentService.Setup(s => s.ApprovePlayerPaymentAsync(1)).ReturnsAsync(dto);

        await CreateController().ApprovePlayerPayment(1);

        _notificationService.Verify(s => s.TriggerPaymentRecordedAsync(It.IsAny<int>()), Times.Never);
    }
}
