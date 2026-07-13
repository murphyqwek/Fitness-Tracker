using MediatR;
using Microsoft.AspNetCore.Mvc;
using Fitness_Tracker_Application.Features.Users.Authorization;

namespace Fitness_Tracker.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthorizationController : Controller
    {
        private readonly IMediator _mediator;

        public AuthorizationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthorizateUserCommand command)
        {
            var result = await _mediator.Send(command);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors.First().Message);
        }
    }
}
