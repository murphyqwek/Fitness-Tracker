using Microsoft.AspNetCore.Mvc;

namespace Fitness_Tracker.Controllers
{
    [Route("api/info")]
    [ApiController]
    public class InfoController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetInfo()
        {
            var info = new
            {
                Application = "Fitness Tracker",
                Version = "0.0.2",
            };

            return Ok(info);
        }
    }
}
