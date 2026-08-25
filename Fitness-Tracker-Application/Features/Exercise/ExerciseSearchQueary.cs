using AutoMapper;
using Fitness_Tracker_Application.Repository.Exercises;
using MediatR;

namespace Fitness_Tracker_Application.Features.Exercise
{
    public record ExerciseSearchCommand(string? Name, IList<int>? MusclesId) : IRequest<IList<ExerciseSearchDTO>>;

    public class ExerciseSearchQueary : IRequestHandler<ExerciseSearchCommand, IList<ExerciseSearchDTO>>
    {
        private readonly IExerciseRepository _repo;
        private readonly ExerciseFuzzySearch _search;
        private readonly IMapper _mapper;

        public ExerciseSearchQueary(IExerciseRepository repo, IMapper mapper, ExerciseFuzzySearch search) 
        {
            _repo = repo;
            _mapper = mapper;
            _search = search;
        }

        public async Task<IList<ExerciseSearchDTO>> Handle(ExerciseSearchCommand request, CancellationToken cancellationToken)
        {
            var result = _search.Search(request.Name);

            return result.ToList();
        }
    }
}
