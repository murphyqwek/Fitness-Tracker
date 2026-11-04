using Fitness_Tracker_Application.DTO.Exercise;
using Fitness_Tracker_Application.Repository.Exercises;
using Fitness_Tracker_Application.Service.Metrics;
using FluentResults;
using MediatR;

namespace Fitness_Tracker_Application.Features.Exercise
{
    public record ExerciseSearchByIdCommand(int Id) : IRequest<Result<ExerciseSearchDTO>>;
    public class GetExerciseByIdQueary : IRequestHandler<ExerciseSearchByIdCommand, Result<ExerciseSearchDTO>>
    {
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IFitnessMetrics _metrics;

        public GetExerciseByIdQueary(IExerciseRepository exerciseRepository, IFitnessMetrics metrics)
        {
            _exerciseRepository = exerciseRepository;
            _metrics = metrics;
        }

        public async Task<Result<ExerciseSearchDTO>> Handle(ExerciseSearchByIdCommand request, CancellationToken cancellationToken)
        {
            using var measurement = _metrics.MeasureExerciseRetrieval(ExerciseRetrievalOperation.ById);
            await _exerciseRepository.FillCacheIfEmpty(cancellationToken);
            return await _exerciseRepository.GetExerciseByIdAsync(request.Id, cancellationToken);
        }
    }
}
