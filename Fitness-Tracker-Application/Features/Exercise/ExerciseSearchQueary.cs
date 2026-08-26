using MediatR;

namespace Fitness_Tracker_Application.Features.Exercise
{
    public record ExerciseSearchCommand(string? Name, IList<int>? MusclesId) : IRequest<IList<ExerciseSearchDTO>>;

    public class ExerciseSearchQueary : IRequestHandler<ExerciseSearchCommand, IList<ExerciseSearchDTO>>
    {
        private readonly ExerciseFuzzySearch _search;

        public ExerciseSearchQueary(ExerciseFuzzySearch search) 
        {
            _search = search;
        }

        public async Task<IList<ExerciseSearchDTO>> Handle(ExerciseSearchCommand request, CancellationToken cancellationToken)
        {
            var result = _search.Search(request.Name, request.MusclesId);

            return result;
        }
    }
}
