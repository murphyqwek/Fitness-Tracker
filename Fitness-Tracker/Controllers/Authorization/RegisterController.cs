using Fitness_Tracker.Services;
using Fitness_Tracker_Application.Features.Users.JWT;
using Fitness_Tracker_Application.Features.Users.Refresh;
using Fitness_Tracker_Application.Features.Users.Registration;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fitness_Tracker.Controllers.Authorization
{
    [Route("api/v1/auth")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RegisterController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsFailed)
            {
                return BadRequest(result.Errors.First().Message);
            }

            var refreshToken = await _mediator.Send(new GenerateRefreshTokenCommand());

            await _mediator.Send(new AddRefreshTokenCommand(refreshToken, result.Value.Id, TimeSpan.FromHours(24)));

            var accessToken = await _mediator.Send(new GenerateJwtTokenCommand(result.Value));

            CookiesHelper.SetAccessAndRefreshTokenCookies(Response, accessToken, refreshToken);

            var userDto = result.Value;

            return Ok(userDto);
        }
    }
}
