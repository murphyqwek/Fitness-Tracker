using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fitness_Tracker.Controllers.Authorization
{
    [Route("api/test-auth")]
    [ApiController]
    public class TestAuthController : ControllerBase
    {
        [Authorize]
        [HttpGet]
        public IActionResult TestAuthorization()
        {
            return Ok(new { Message = "You are authorized!" });
        }
    }
}
