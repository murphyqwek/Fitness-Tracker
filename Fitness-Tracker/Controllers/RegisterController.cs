using Fitness_Tracker.Services;
using Fitness_Tracker_Application.Features.Users.Registration;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fitness_Tracker.Controllers
{
    [Route("api/register")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RegisterController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserCommand command, [FromServices] GenerateJwtTokenService jwtService)
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
