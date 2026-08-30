using Fitness_Tracker_Application.DTO.Workout;
using Fitness_Tracker_Application.Repository.Workout;
using FluentResults;
using MediatR;

namespace Fitness_Tracker_Application.Features.Workout
{
    public record GetWorkoutByIdCommand(Guid userId, Guid workoutId) : IRequest<Result<ResponseWorkoutDTO>>;
    public class GetWorkoutById : IRequestHandler<GetWorkoutByIdCommand, Result<ResponseWorkoutDTO>>
    {
        private readonly IWorkoutRepository _repo;

        public GetWorkoutById(IWorkoutRepository repo)
        {
            _repo = repo;
        }

        public Task<Result<ResponseWorkoutDTO>> Handle(GetWorkoutByIdCommand request, CancellationToken cancellationToken)
        {
            return _repo.GetWorkoutById(request.userId, request.workoutId, cancellationToken);
        }
    }
}
