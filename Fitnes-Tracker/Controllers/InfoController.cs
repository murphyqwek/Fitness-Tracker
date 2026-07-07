using Microsoft.AspNetCore.Mvc;

namespace Fintes_Tracker.Controllers
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
                Application = "Fintes Tracker",
                Version = "0.0.2",
            };

            return Ok(info);
        }
    }
}
