using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Badminton_BE.Controllers
{
    /// <summary>
    /// Manage sessions (create, read, update).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SessionController : ControllerBase
    {
        public SessionController()
        {
        }

        /// <summary>
        /// Create a new session.
        /// </summary>
        /// <returns>Confirmation message.</returns>
        [HttpPost]
        //[SwaggerOperation(Summary = "Create session", Description = "Creates a new session.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Session created successfully")]
        public IActionResult CreateSession()
        {
            return Ok("Session created successfully");
        }

        /// <summary>
        /// Get all sessions.
        /// </summary>
        [HttpGet]
        //[SwaggerOperation(Summary = "List sessions", Description = "Returns a list of sessions.")]
        [SwaggerResponse(StatusCodes.Status200OK, "A list of sessions")]
        public IActionResult GetSessions()
        {
            return Ok();
        }

        /// <summary>
        /// Get a session by id.
        /// </summary>
        /// <param name="id">Session identifier.</param>
        [HttpGet()]
        [Route("{id}")]
        //[SwaggerOperation(Summary = "Get session by id", Description = "Returns a single session by id.")]
        [SwaggerResponse(StatusCodes.Status200OK, "The session")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Session not found")]
        public IActionResult GetSessionById(int id)
        {
            return Ok();
        }

        /// <summary>
        /// Update a session.
        /// </summary>
        /// <param name="id">Session identifier.</param>
        [HttpPut("{id}")]
        //[SwaggerOperation(Summary = "Update session", Description = "Updates the session with the given id.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Session updated")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Session not found")]
        public IActionResult UpdateSession(int id)
        {
            return Ok();
        }
    }
}
