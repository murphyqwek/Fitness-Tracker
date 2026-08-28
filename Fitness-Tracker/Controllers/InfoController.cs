using Microsoft.AspNetCore.Mvc;

namespace Fitness_Tracker.Controllers
{
    [Route("api/v1/info")]
    [ApiController]
    public class InfoController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetInfo()
        {
            var info = new
            {
                Application = "Fitness Tracker",
                Version = "0.0.5",
            };

            return Ok(info);
        }
    }
}
