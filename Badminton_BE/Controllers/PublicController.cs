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
    /// Publicly accessible endpoints — no authentication required.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class PublicController : ControllerBase
    {
        private readonly IMemberService _memberService;

        public PublicController(IMemberService memberService)
        {
            _memberService = memberService;
        }

        /// <summary>
        /// Get member session history (public, no auth needed).
        /// </summary>
        /// <param name="id">Member identifier.</param>
        [HttpGet("member/{id}/sessions")]
        [SwaggerResponse(StatusCodes.Status200OK, "Member sessions found", typeof(IEnumerable<MemberLookupSessionDto>))]
        public async Task<IActionResult> GetMemberSessions(int id)
        {
            // Always return ALL sessions for anonymous callers
            var sessions = await _memberService.GetMemberSessionsAsync(id, null);
            return Ok(sessions);
        }
    }
}
