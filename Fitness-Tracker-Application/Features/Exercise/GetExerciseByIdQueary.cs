using FluentResults;
using MediatR;

namespace Fitness_Tracker_Application.Features.Exercise
{
    public record ExerciseSearchByIdCommand(int Id) : IRequest<Result<ExerciseSearchDTO>>;
    public class GetExerciseByIdQueary : IRequestHandler<ExerciseSearchByIdCommand, Result<ExerciseSearchDTO>>
    {
        private readonly ExerciseFuzzySearch _search;

        public GetExerciseByIdQueary(ExerciseFuzzySearch search) 
        {
            _search = search;
        }

        public async Task<Result<ExerciseSearchDTO>> Handle(ExerciseSearchByIdCommand request, CancellationToken cancellationToken)
        {
            return _search.GetExerciseById(request.Id);
        }
    }
}
