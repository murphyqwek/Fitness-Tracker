using Fitness_Tracker_Application.DTO.Exercise;
using Fitness_Tracker_Application.Repository.Exercises;
using Fitness_Tracker_Application.Service.Pagination;
using MediatR;

namespace Fitness_Tracker_Application.Features.Exercise
{
    public record ExerciseSearchCommand(string? Name, IList<int>? MusclesId) : IRequest<PaginationResponse<ExerciseSearchReducedDTO>>, IPaginationCommand
    {
        public int Page { get; set; }
        public int Size { get; set; }
    }

    public class ExerciseSearchQueary : IRequestHandler<ExerciseSearchCommand, PaginationResponse<ExerciseSearchReducedDTO>>
    {
        private readonly IExerciseRepository _exerciseRepository;

        public ExerciseSearchQueary(IExerciseRepository exerciseRepository) 
        {
            _exerciseRepository = exerciseRepository;
        }

        public async Task<PaginationResponse<ExerciseSearchReducedDTO>> Handle(ExerciseSearchCommand request, CancellationToken cancellationToken)
        {
            var result = await _exerciseRepository.GetExerciseAsync(request.Name, request.MusclesId, request.Page, request.Size, cancellationToken);

            return result;
        }
    }
}
