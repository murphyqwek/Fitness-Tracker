using Microsoft.AspNetCore.Mvc;

namespace Fitness_Tracker.Controllers.User
{
    [Route("api/v1/user/me")]
    [ApiController]
    public class UserMeController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetUserInfo() {
            
        }
    }
}
