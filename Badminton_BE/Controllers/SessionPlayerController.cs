using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Badminton_BE.Services;
using Badminton_BE.DTOs;
using Swashbuckle.AspNetCore.Annotations;

namespace Badminton_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionPlayerController : ControllerBase
    {
        private readonly ISessionPlayerService _service;

        public SessionPlayerController(ISessionPlayerService service)
        {
            _service = service;
        }

        /// <summary>
        /// Add a member to a session. Default status is Joined.
        /// </summary>
        [HttpPost]
        [SwaggerResponse(StatusCodes.Status201Created, "Member added to session", typeof(SessionPlayerReadDto))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Session or Member not found")]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Member already added to session")]
        public async Task<IActionResult> AddMemberToSession([FromBody] SessionPlayerCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var created = await _service.AddMemberToSessionAsync(dto);
            if (created == null)
            {
                // Could be not found or conflict (duplicate or overlap). Return Conflict with a generic message.
                return Conflict("Could not add member to session (not found, already exists, or overlapping session).");
            }

            return CreatedAtAction(nameof(GetSessionPlayer), new { id = created.Id }, created);
        }

        /// <summary>
        /// Update the status of a session player (e.g., mark as Paid or Canceled).
        /// </summary>
        [HttpPatch("{id}/status")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Status updated")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Session player not found")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] DTOs.SessionPlayerStatusUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var ok = await _service.ChangeStatusAsync(id, dto.Status);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpGet("{id}")]
        [SwaggerResponse(StatusCodes.Status200OK, "Session player found", typeof(SessionPlayerReadDto))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Session player not found")]
        public async Task<IActionResult> GetSessionPlayer(int id)
        {
            var sp = await _service.GetByIdAsync(id);
            if (sp == null) return NotFound();
            return Ok(sp);
        }

        /// <summary>
        /// Remove a member from a session by session-player id.
        /// </summary>
        [HttpDelete("{id}")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Member removed from session")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Session player not found")]
        public async Task<IActionResult> Remove(int id)
        {
            var ok = await _service.RemoveAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
