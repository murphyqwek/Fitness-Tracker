using AutoMapper;
using Fitness_Tracker_Application.Repository.Exercises;
using FluentResults;
using MediatR;

namespace Fitness_Tracker_Application.Features.Exercise
{
    public record ExerciseSearchByIdCommand(int Id) : IRequest<Result<ExerciseSearchDTO>>;
    public class GetExerciseByIdQueary : IRequestHandler<ExerciseSearchByIdCommand, Result<ExerciseSearchDTO>>
    {
        private readonly IExerciseRepository _repo;
        private readonly IMapper _mapper;

        public GetExerciseByIdQueary(IExerciseRepository repo, IMapper mapper) 
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<Result<ExerciseSearchDTO>> Handle(ExerciseSearchByIdCommand request, CancellationToken cancellationToken)
        {
            var result = await _repo.GetExerciseByIdAsync(request.Id, cancellationToken);

            if(result.IsFailed) {
                return Result.Fail($"Exercise with id {request.Id} does not exist");
            }

            return result;
        }
    }
}
