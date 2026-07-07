using MediatR;
using Microsoft.AspNetCore.Mvc;
using Fintess_Tracker_Application.Features.Users.Authorization;

namespace Fintes_Tracker.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthorizationController : Controller
    {
        private readonly Mediator _mediator;

        public AuthorizationController(Mediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthorizateUserCommand command)
        {
            var result = await _mediator.Send(command);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
        }
    }
}
