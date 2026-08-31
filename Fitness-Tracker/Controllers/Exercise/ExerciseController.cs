using Fitness_Tracker_Application.DTO.Exercise;
using Fitness_Tracker_Application.Features.Exercise;
using Fitness_Tracker_Application.Service.Pagination;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;


namespace Fitness_Tracker.Controllers.Exercise
{
    [Route("api/v1/exercise")]
    [ApiController]
    public class ExerciseController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ExerciseController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<PaginationResponse<ExerciseSearchReducedDTO>> Get([FromQuery] string? name, [FromQuery] List<int> muscleIds, int? page, int? size, CancellationToken cancellationToken)
        {
            page ??= 1;
            size ??= 10;

            return await _mediator.Send(new ExerciseSearchCommand(name, muscleIds) { Page = page.Value, Size = size.Value }, cancellationToken);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ExerciseSearchByIdCommand(id), cancellationToken);

            if (result.IsFailed)
            {
                return NotFound(result.Errors.First().Message);
            }

            return Ok(result.Value);
        }

        [HttpPost("fill")]
        [AllowAnonymous]
        public async Task<IActionResult> FillCache(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new FillCacheExerciseCommand(), cancellationToken);

            if (result.IsFailed)
            {
                return BadRequest();
            }

            return Ok();
        }

    }
}
