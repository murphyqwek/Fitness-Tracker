using Fitness_Tracker_Application.Service.Pagination;
using MediatR;

namespace Fitness_Tracker_Application.Features.Exercise
{
    public record ExerciseSearchCommand(string? Name, IList<int>? MusclesId) : IRequest<PaginationResponse<ExerciseSearchDTO>>, IPaginationCommand
    {
        public int Page { get; set; }
        public int Size { get; set; }
    }

    public class ExerciseSearchQueary : IRequestHandler<ExerciseSearchCommand, PaginationResponse<ExerciseSearchDTO>>
    {
        private readonly ExerciseFuzzySearch _search;

        public ExerciseSearchQueary(ExerciseFuzzySearch search) 
        {
            _search = search;
        }

        public async Task<PaginationResponse<ExerciseSearchDTO>> Handle(ExerciseSearchCommand request, CancellationToken cancellationToken)
        {
            var result = _search.SearchByPage(request.Name, request.MusclesId, request.Page, request.Size);

            return result;
        }
    }
}
