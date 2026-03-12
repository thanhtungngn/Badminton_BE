using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Badminton_BE.Repositories;
using Badminton_BE.Services;
using Badminton_BE.DTOs;
using Badminton_BE.Models;

namespace Badminton_BE.Controllers
{
    /// <summary>
    /// Manage sessions (create, read, update).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SessionController : ControllerBase
    {
        private readonly ISessionService _service;

        public SessionController(ISessionService service)
        {
            _service = service;
        }

        /// <summary>
        /// Create a new session.
        /// </summary>
        /// <returns>Created session.</returns>
        [HttpPost]
        [SwaggerResponse(StatusCodes.Status201Created, "Session created successfully", typeof(SessionReadDto))]
        public async Task<IActionResult> CreateSession([FromBody] SessionCreateDto dto)
        {
            var read = await _service.CreateSessionAsync(dto);
            return CreatedAtAction(nameof(GetSessionById), new { id = read.Id }, read);
        }

        /// <summary>
        /// Get all sessions.
        /// </summary>
        [HttpGet]
        [SwaggerResponse(StatusCodes.Status200OK, "A list of sessions", typeof(IEnumerable<SessionReadDto>))]
        public async Task<IActionResult> GetSessions()
        {
            var sessions = await _service.GetSessionsAsync();
            return Ok(sessions);
        }

        /// <summary>
        /// Get sessions for dashboard: only upcoming and ongoing sessions.
        /// </summary>
        [HttpGet("dashboard")]
        [SwaggerResponse(StatusCodes.Status200OK, "A list of active sessions for dashboard", typeof(IEnumerable<SessionReadDto>))]
        public async Task<IActionResult> GetActiveSessions()
        {
            var sessions = await _service.GetActiveSessionsAsync();
            return Ok(sessions);
        }

        /// <summary>
        /// Get a session by id.
        /// </summary>
        /// <param name="id">Session identifier.</param>
        [HttpGet()]
        [Route("{id}")]
        [SwaggerResponse(StatusCodes.Status200OK, "The session", typeof(SessionReadDto))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Session not found")]
        public async Task<IActionResult> GetSessionById(int id)
        {
            var read = await _service.GetSessionByIdAsync(id);
            if (read == null) return NotFound();
            return Ok(read);
        }

        /// <summary>
        /// Get session details including players and member info.
        /// </summary>
        [HttpGet]
        [Route("{id}/detail")]
        [SwaggerResponse(StatusCodes.Status200OK, "Session detail", typeof(SessionWithPlayersDto))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Session not found")]
        public async Task<IActionResult> GetSessionDetail(int id)
        {
            var detail = await _service.GetSessionDetailAsync(id);
            if (detail == null) return NotFound();
            return Ok(detail);
        }

        /// <summary>
        /// Update a session.
        /// </summary>
        /// <param name="id">Session identifier.</param>
        [HttpPut("{id}")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Session updated")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Session not found")]
        public async Task<IActionResult> UpdateSession(int id, [FromBody] SessionUpdateDto dto)
        {
            var updated = await _service.UpdateSessionAsync(id, dto);
            if (!updated) return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Delete a session by id.
        /// </summary>
        [HttpDelete("{id}")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Session deleted")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Session not found")]
        public async Task<IActionResult> DeleteSession(int id)
        {
            var deleted = await _service.DeleteSessionAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
