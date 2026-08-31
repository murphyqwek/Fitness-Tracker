using Fitness_Tracker_Application.DTO.Exercise;
using Fitness_Tracker_Application.Repository.Exercises;
using FluentResults;
using MediatR;

namespace Fitness_Tracker_Application.Features.Exercise
{
    public record ExerciseSearchByIdCommand(int Id) : IRequest<Result<ExerciseSearchDTO>>;
    public class GetExerciseByIdQueary : IRequestHandler<ExerciseSearchByIdCommand, Result<ExerciseSearchDTO>>
    {
        private readonly IExerciseRepository _exerciseRepository;

        public GetExerciseByIdQueary(IExerciseRepository exerciseRepository) 
        {
            _exerciseRepository = exerciseRepository;
        }

        public async Task<Result<ExerciseSearchDTO>> Handle(ExerciseSearchByIdCommand request, CancellationToken cancellationToken)
        {
            return await _exerciseRepository.GetExerciseByIdAsync(request.Id, cancellationToken);
        }
    }
}
