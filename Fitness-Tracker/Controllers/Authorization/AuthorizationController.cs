using Fitness_Tracker.DTO;
using Fitness_Tracker.Services;
using Fitness_Tracker_Application.Features.Users.Authorization;
using Fitness_Tracker_Application.Features.Users.JWT;
using Fitness_Tracker_Application.Features.Users.Refresh;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fitness_Tracker.Controllers.Authorization
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
        public async Task<IActionResult> Login([FromBody] AuthorizateUserCommand command)
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

            var userDTO = result.Value;

            return Ok(new UserAuthResponse(userDTO.Id, userDTO.Login));
        }
    }
}
