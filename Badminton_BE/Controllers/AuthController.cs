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
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

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
    }
}
