using Fitness_Tracker_Application.DTO.Workout;
using Fitness_Tracker_Application.Features.Workout;
using Fitness_Tracker_Application.Repository.Workout;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fitness_Tracker.Controllers.Workout
{
    [Route("api/v1/workout")]
    [ApiController]
    public class WorkoutController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WorkoutController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetTimeline([FromQuery] DateTimeOffset? cursor, [FromQuery] int limit = 10)
        {
            var userId = User.GetUserId();

            var query = new GetWorkoutsQuery(userId, cursor, limit);
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(Guid id)
        {
            var userId = User.GetUserId();

            var result = await _mediator.Send(new GetWorkoutByIdCommand(userId, id));

            if(result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return NotFound(result.Errors.First().Message);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromHeader(Name = "Idempotency-Key")] string idempotencyKeyStr, [FromBody] CreateWorkoutDTO createRequest)
        {
            var idempotencyKey = new Guid(idempotencyKeyStr);
            var userId = User.GetUserId();

            var result = await _mediator.Send(new CreateWorkoutCommand(userId, idempotencyKey, createRequest));

            if(result.IsSuccess)
            {
                var id = result.Value;
                return Created($"/workout/{id}", id);
            }

            var error = result.Errors.First().Message;

            if (error == "Request contains exercise's ids that does not exist in database")
            {
                return BadRequest(error);
            }

            if (error == "Database error") 
            {
                return StatusCode(500, "Database error was acuired during saving the workout");
            }

            if(error == IdempotencyStatus.RUNNING.ToString())
            {
                return Conflict();
            }

            if (error == IdempotencyStatus.UNKNOW.ToString())
            {
                return StatusCode(500, "Unexpected error was acuired during saving the workout (couldn't get lock)");
            }

            

            return StatusCode(500, "Unexpected error was acuired during saving the workout");
        }
    }
}
