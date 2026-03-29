using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Badminton_BE.DTOs;
using Badminton_BE.Services.Interfaces;

namespace Badminton_BE.Controllers
{
    [ApiController]
    [Route("api/session/{sessionId}/matches")]
    public class SessionMatchController : ControllerBase
    {
        private readonly ISessionMatchService _service;

        public SessionMatchController(ISessionMatchService service)
        {
            _service = service;
        }

        [HttpGet]
        [SwaggerResponse(StatusCodes.Status200OK, "Session matches", typeof(IEnumerable<SessionMatchReadDto>))]
        public async Task<IActionResult> GetMatches(int sessionId)
        {
            var matches = await _service.GetBySessionIdAsync(sessionId);
            return Ok(matches);
        }

        [HttpGet("{matchId}")]
        [SwaggerResponse(StatusCodes.Status200OK, "Match found", typeof(SessionMatchReadDto))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Match not found")]
        public async Task<IActionResult> GetMatch(int sessionId, int matchId)
        {
            var match = await _service.GetByIdAsync(sessionId, matchId);
            if (match == null) return NotFound();
            return Ok(match);
        }

        [HttpPost]
        [SwaggerResponse(StatusCodes.Status201Created, "Match created", typeof(SessionMatchReadDto))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid match data")]
        public async Task<IActionResult> CreateMatch(int sessionId, [FromBody] SessionMatchUpsertDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var match = await _service.CreateAsync(sessionId, dto);
            if (match == null)
            {
                return BadRequest("Could not create match. Ensure both teams contain 1 to 2 unique session players and winner matches the score.");
            }

            return CreatedAtAction(nameof(GetMatch), new { sessionId, matchId = match.Id }, match);
        }

        [HttpPut("{matchId}")]
        [SwaggerResponse(StatusCodes.Status200OK, "Match updated", typeof(SessionMatchReadDto))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid match data")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Match not found")]
        public async Task<IActionResult> UpdateMatch(int sessionId, int matchId, [FromBody] SessionMatchUpsertDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = await _service.GetByIdAsync(sessionId, matchId);
            if (existing == null) return NotFound();

            var updated = await _service.UpdateAsync(sessionId, matchId, dto);
            if (updated == null)
            {
                return BadRequest("Could not update match. Ensure both teams contain 1 to 2 unique session players and winner matches the score.");
            }

            return Ok(updated);
        }

        [HttpDelete("{matchId}")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Match deleted")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Match not found")]
        public async Task<IActionResult> DeleteMatch(int sessionId, int matchId)
        {
            var deleted = await _service.DeleteAsync(sessionId, matchId);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
