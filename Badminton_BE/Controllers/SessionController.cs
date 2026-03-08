using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Badminton_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionController : ControllerBase
    {
        public SessionController()
        {
            
        }

        [HttpPost]
        public IActionResult CreateSession()
        {
            return Ok("Session created successfully");
        }

        [HttpGet]
        public IActionResult GetSessions()
        {
            return Ok();
        }

        [HttpGet]
        public IActionResult GetSessionById(int id)
        {
            return Ok();
        }

        [HttpPut]
        public IActionResult UpdateSession(int id)
        {
            return Ok();
        }
    }
}
