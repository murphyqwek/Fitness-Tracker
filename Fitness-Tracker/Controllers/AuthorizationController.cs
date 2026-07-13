using Fitness_Tracker.Services;
using Fitness_Tracker_Application.Features.Users.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] AuthorizateUserCommand command, [FromServices] GenerateJwtTokenService jwtService)
        {
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                var token = jwtService.Generate(result.Value);

                Response.Cookies.Append("accessToken", token, new CookieOptions
                {
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    HttpOnly = true,
                });

                return Ok();
            }

            return BadRequest(result.Errors.First().Message);
        }
    }
}
