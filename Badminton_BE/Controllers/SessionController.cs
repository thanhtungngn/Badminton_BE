using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Badminton_BE.Repositories;
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
        private readonly ISessionRepository _repo;

        public SessionController(ISessionRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Create a new session.
        /// </summary>
        /// <returns>Created session.</returns>
        [HttpPost]
        [SwaggerResponse(StatusCodes.Status201Created, "Session created successfully", typeof(SessionReadDto))]
        public async Task<IActionResult> CreateSession([FromBody] SessionCreateDto dto)
        {
            var s = new Session
            {
                Title = dto.Title,
                Description = dto.Description,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Location = dto.Location
            };

            await _repo.AddAsync(s);
            await _repo.SaveChangesAsync();

            var read = new SessionReadDto
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                Location = s.Location
            };

            return CreatedAtAction(nameof(GetSessionById), new { id = read.Id }, read);
        }

        /// <summary>
        /// Get all sessions.
        /// </summary>
        [HttpGet]
        [SwaggerResponse(StatusCodes.Status200OK, "A list of sessions", typeof(IEnumerable<SessionReadDto>))]
        public async Task<IActionResult> GetSessions()
        {
            var sessions = (await _repo.GetAllAsync())
                .Select(s => new SessionReadDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    Description = s.Description,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    Location = s.Location
                });

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
            var s = await _repo.GetByIdAsync(id);
            if (s == null) return NotFound();

            var read = new SessionReadDto
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                Location = s.Location
            };

            return Ok(read);
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
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();

            existing.Title = dto.Title;
            existing.Description = dto.Description;
            existing.StartTime = dto.StartTime;
            existing.EndTime = dto.EndTime;
            existing.Location = dto.Location;

            _repo.Update(existing);
            await _repo.SaveChangesAsync();

            return NoContent();
        }
    }
}
