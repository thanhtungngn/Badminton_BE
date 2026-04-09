using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Badminton_BE.Services;
using Badminton_BE.Services.Interfaces;
using Badminton_BE.DTOs;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Http;

namespace Badminton_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _service;
        private readonly INotificationService _notificationService;

        public PaymentController(IPaymentService service, INotificationService notificationService)
        {
            _service = service;
            _notificationService = notificationService;
        }

        [HttpPost("session/{sessionId}")]
        [SwaggerResponse(StatusCodes.Status200OK, "Session prices set", typeof(object))]
        public async Task<IActionResult> SetSessionPrices(int sessionId, [FromBody] SessionPaymentCreateDto dto)
        {
            var sp = await _service.SetSessionPricesAsync(sessionId, dto.PriceMale, dto.PriceFemale);
            if (sp == null) return NotFound();
            return Ok(sp);
        }


        [HttpPost("session-player/{sessionPlayerId}/pay")]
        [SwaggerResponse(StatusCodes.Status200OK, "Payment applied", typeof(PlayerPaymentReadDto))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Session player or price config not found")]
        public async Task<IActionResult> PayBySessionPlayer(int sessionPlayerId, [FromBody] PlayerPaymentPayDto dto)
        {
            var r = await _service.PayBySessionPlayerIdAsync(sessionPlayerId, dto.Amount);
            if (r == null) return NotFound();
            return Ok(r);
        }

        [HttpPut("session-player/{sessionPlayerId}/amount")]
        [SwaggerResponse(StatusCodes.Status200OK, "Amount due updated", typeof(PlayerPaymentReadDto))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Player payment not found")]
        public async Task<IActionResult> UpdatePlayerPaymentAmount(int sessionPlayerId, [FromBody] PlayerPaymentUpdateAmountDto dto)
        {
            var r = await _service.UpdateAmountDueAsync(sessionPlayerId, dto.AmountDue);
            if (r == null) return NotFound();
            return Ok(r);
        }

        /// <summary>
        /// Player signals they have paid. Sets status to ConfirmationPending — awaiting owner approval.
        /// </summary>
        [HttpPost("session-player/{sessionPlayerId}/confirm")]
        [SwaggerResponse(StatusCodes.Status200OK, "Payment pending confirmation", typeof(PlayerPaymentReadDto))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Player payment not found")]
        public async Task<IActionResult> ConfirmPlayerPayment(int sessionPlayerId)
        {
            var result = await _service.ConfirmPlayerPaymentAsync(sessionPlayerId);
            if (result == null) return NotFound();

            await _notificationService.TriggerPaymentRecordedAsync(sessionPlayerId);

            return Ok(result);
        }

        /// <summary>
        /// Owner approves a player's pending payment. Sets status to Paid.
        /// </summary>
        [HttpPost("session-player/{sessionPlayerId}/approve")]
        [SwaggerResponse(StatusCodes.Status200OK, "Payment approved and marked as Paid", typeof(PlayerPaymentReadDto))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Player payment not found")]
        public async Task<IActionResult> ApprovePlayerPayment(int sessionPlayerId)
        {
            var result = await _service.ApprovePlayerPaymentAsync(sessionPlayerId);
            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}
