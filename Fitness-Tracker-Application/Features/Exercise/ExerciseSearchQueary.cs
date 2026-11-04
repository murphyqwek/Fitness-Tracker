using Fitness_Tracker_Application.DTO.Exercise;
using Fitness_Tracker_Application.Repository.Exercises;
using Fitness_Tracker_Application.Service.Pagination;
using Fitness_Tracker_Application.Service.Metrics;
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
        private readonly IFitnessMetrics _metrics;

        public ExerciseSearchQueary(IExerciseRepository exerciseRepository, IFitnessMetrics metrics)
        {
            _exerciseRepository = exerciseRepository;
            _metrics = metrics;
        }

        public async Task<PaginationResponse<ExerciseSearchReducedDTO>> Handle(ExerciseSearchCommand request, CancellationToken cancellationToken)
        {
            using var measurement = _metrics.MeasureExerciseRetrieval(ExerciseRetrievalOperation.Search);
            await _exerciseRepository.FillCacheIfEmpty(cancellationToken);

            var result = await _exerciseRepository.GetExerciseAsync(request.Name, request.MusclesId, request.Page, request.Size, cancellationToken);

            return result;
        }
    }
}
