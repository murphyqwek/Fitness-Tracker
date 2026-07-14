using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Fitness_Tracker_Application.Features.Users.Refresh;
using Fitness_Tracker_Application.Features.Users;
using Fitness_Tracker.Services;

namespace Fitness_Tracker.Controllers
{
    [Route("api/auth/refresh")]
    [ApiController]
    public class RefreshController : ControllerBase
    {
        private readonly IMediator _mediator;
        public RefreshController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            var result = await _mediator.Send(new RefreshTokenCommand(refreshToken));

            if(result.IsFailed)
            {
                return Unauthorized(result.Errors.First().Message);
            }

            var tokens = result.Value;

            CookiesHelper.SetAccessAndRefreshTokenCookies(Response, tokens.AccessToken, tokens.RefreshToken);

            return Ok();
        }
    }
}
