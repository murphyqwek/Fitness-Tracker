using Fitness_Tracker_Application.Features.Exercise;
using MediatR;
using Microsoft.AspNetCore.Mvc;


namespace Fitness_Tracker.Controllers.Exercise
{
    [Route("api/exercise")]
    [ApiController]
    public class ExerciseController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ExerciseController(IMediator mediator) 
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IList<ExerciseSearchDTO>> Get(CancellationToken cancellationToken)
        {
            return await _mediator.Send(new ExerciseSearchCommand(null, null), cancellationToken);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ExerciseSearchByIdCommand(id), cancellationToken);

            if(result.IsFailed) 
            {
                return NotFound(result.Errors.First().Message);
            }

            return Ok(result.Value);
        }

    }
}
