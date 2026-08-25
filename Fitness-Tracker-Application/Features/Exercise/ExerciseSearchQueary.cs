using AutoMapper;
using Fitness_Tracker_Application.Repository.Exercises;
using MediatR;

namespace Fitness_Tracker_Application.Features.Exercise
{
    public record ExerciseSearchCommand(string? Name, IList<int>? MusclesId) : IRequest<IList<ExerciseSearchDTO>>;

    public class ExerciseSearchQueary : IRequestHandler<ExerciseSearchCommand, IList<ExerciseSearchDTO>>
    {
        private readonly IExerciseRepository _repo;
        private readonly IMapper _mapper;

        public ExerciseSearchQueary(IExerciseRepository repo, IMapper mapper) 
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IList<ExerciseSearchDTO>> Handle(ExerciseSearchCommand request, CancellationToken cancellationToken)
        {
            var result = await _repo.GetExerciseAsync(request.Name, request.MusclesId, cancellationToken);

            return _mapper.Map<IList<ExerciseSearchDTO>>(result);
        }
    }
}
