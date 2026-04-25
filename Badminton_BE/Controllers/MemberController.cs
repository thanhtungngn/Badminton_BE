using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Badminton_BE.Services;
using Badminton_BE.DTOs;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace Badminton_BE.Controllers
{
    /// <summary>
    /// Manage members and their contacts.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MemberController : ControllerBase
    {
        private readonly IMemberService _service;

        public MemberController(IMemberService service)
        {
            _service = service;
        }

        /// <summary>
        /// Create a new member along with optional contacts.
        /// </summary>
        /// <param name="dto">Member creation data.</param>
        [HttpPost]
        [SwaggerResponse(StatusCodes.Status201Created, "Member created", typeof(MemberReadDto))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request")]
        public async Task<IActionResult> CreateMember([FromBody] MemberCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var created = await _service.CreateMemberAsync(dto);
            return CreatedAtAction(nameof(GetMemberById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Get all members with their contacts.
        /// </summary>
        [HttpGet]
        [SwaggerResponse(StatusCodes.Status200OK, "List of members", typeof(IEnumerable<MemberReadDto>))]
        public async Task<ActionResult<IEnumerable<MemberReadDto>>> GetMembers()
        {
            var members = await _service.GetMembersAsync();
            return Ok(members);
        }

        /// <summary>
        /// Get a member by a contact value (phone, email, etc.).
        /// </summary>
        /// <param name="contactValue">Contact value to search for.</param>
        [HttpGet("by-contact")]
        [SwaggerResponse(StatusCodes.Status200OK, "Member found", typeof(MemberReadDto))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Member not found")]
        public async Task<IActionResult> GetMemberByContact([FromQuery] string contactValue)
        {
            if (string.IsNullOrWhiteSpace(contactValue)) return BadRequest("contactValue is required");

            var m = await _service.GetMemberByContactValueAsync(contactValue);
            if (m == null) return NotFound();
            return Ok(m);
        }

        /// <summary>
        /// Look up a user's sessions, payment status, elo, and level by one of their contacts.
        /// </summary>
        /// <param name="contactValue">Contact value to search for.</param>
        [AllowAnonymous]
        [HttpGet("lookup")]
        [SwaggerResponse(StatusCodes.Status200OK, "Member lookup result", typeof(MemberLookupDto))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Member not found")]
        public async Task<IActionResult> LookupMember([FromQuery] string contactValue)
        {
            if (string.IsNullOrWhiteSpace(contactValue)) return BadRequest("contactValue is required");

            var result = await _service.GetMemberLookupByContactAsync(contactValue);
            if (result == null) return NotFound();
            return Ok(result);
        }

        /// <summary>
        /// Get a member by id.
        /// </summary>
        /// <param name="id">Member identifier.</param>
        [HttpGet("{id}")]
        [SwaggerResponse(StatusCodes.Status200OK, "Member found", typeof(MemberReadDto))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Member not found")]
        public async Task<IActionResult> GetMemberById(int id)
        {
            var m = await _service.GetMemberByIdAsync(id);
            if (m == null) return NotFound();
            return Ok(m);
        }

        /// <summary>
        /// Update a member and its contacts.
        /// </summary>
        /// <param name="id">Member identifier.</param>
        [HttpPut("{id}")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Member updated")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Member not found")]
        public async Task<IActionResult> UpdateMember(int id, [FromBody] MemberUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updated = await _service.UpdateMemberAsync(id, dto);
            if (!updated) return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Delete a member and their contacts.
        /// </summary>
        /// <param name="id">Member identifier.</param>
        [HttpDelete("{id}")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Member deleted")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Member not found")]
        public async Task<IActionResult> DeleteMember(int id)
        {
            var deleted = await _service.DeleteMemberAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
        [AllowAnonymous]
        [HttpPatch("{id}/nickname")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Nickname updated")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Member not found")]
        public async Task<IActionResult> UpdateNickname(int id, [FromBody] NicknameUpdateDto dto)
        {
            var updated = await _service.UpdateNicknameAsync(id, dto.Nickname);
            if (!updated) return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Look up a member's profile by id (public).
        /// </summary>
        /// <param name="memberId">Member identifier.</param>
        [AllowAnonymous]
        [HttpGet("lookup-id")]
        [SwaggerResponse(StatusCodes.Status200OK, "Member found", typeof(MemberLookupDto))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Member not found")]
        public async Task<IActionResult> LookupById([FromQuery] int memberId)
        {
            var result = await _service.GetMemberByIdForLookupAsync(memberId);
            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}
