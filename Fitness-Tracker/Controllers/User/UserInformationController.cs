using Fitness_Tracker_Application.DTO.User;
using Fitness_Tracker_Application.Features.Users.Infomration;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fitness_Tracker.Controllers.User
{
    [Route("api/v1/user/me")]
    [ApiController]
    public class UserInformationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserInformationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserInfo(CancellationToken cancellationToken) {
            var id = User.GetUserId();

            var result = await _mediator.Send(new GetUserInformationCommand(id), cancellationToken);

            if(result.IsFailed) 
            {
                return NotFound(result.Errors.First().Message);
            }

            return Ok(result.Value);
        }

        [HttpPatch]
        [Authorize]
        public async Task<IActionResult> PatchUserInfo([FromBody] UserUpdateDTO userUpdateDTO, CancellationToken cancellationToken) 
        {
            var id = User.GetUserId();

            var result = await _mediator.Send(new UpdateUserInformationCommand(id, userUpdateDTO), cancellationToken);

            if (result.IsFailed)
            {
                return NotFound(result.Errors.First().Message);
            }

            return Ok();
        }
    }
}
