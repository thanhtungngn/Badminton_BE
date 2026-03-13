using System.Threading.Tasks;
using Badminton_BE.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Badminton_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        [SwaggerResponse(StatusCodes.Status200OK, "Account created", typeof(AuthResponseDto))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request")]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Username already exists")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(dto);
            if (result == null) return Conflict("Username already exists.");

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [SwaggerResponse(StatusCodes.Status200OK, "Login successful", typeof(AuthResponseDto))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Invalid username or password")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.LoginAsync(dto);
            if (result == null) return Unauthorized("Invalid username or password.");

            return Ok(result);
        }

        [Authorize]
        [HttpPost("logout")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Logout successful")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized")]
        public async Task<IActionResult> Logout()
        {
            var userIdValue = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var jti = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)?.Value;
            var expValue = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Exp)?.Value;

            if (!int.TryParse(userIdValue, out var userId) || string.IsNullOrWhiteSpace(jti) || !long.TryParse(expValue, out var expUnix))
            {
                return Unauthorized();
            }

            var expiresAt = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
            await _authService.LogoutAsync(userId, jti, expiresAt);
            return NoContent();
        }

        [Authorize]
        [HttpGet("profile")]
        [SwaggerResponse(StatusCodes.Status200OK, "User profile", typeof(UserProfileReadDto))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
        public async Task<IActionResult> GetProfile()
        {
            var userIdValue = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var profile = await _authService.GetProfileAsync(userId);
            if (profile == null) return NotFound();

            return Ok(profile);
        }

        [Authorize]
        [HttpPut("profile")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Profile updated")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserProfileUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userIdValue = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var updated = await _authService.UpdateProfileAsync(userId, dto);
            if (!updated) return NotFound();

            return NoContent();
        }
    }
}
