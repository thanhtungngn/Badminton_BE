using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Badminton_BE.Services;
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

        public PaymentController(IPaymentService service)
        {
            _service = service;
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
    }
}
