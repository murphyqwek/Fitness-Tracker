using Fitness_Tracker_Application.DTO.Workout;
using FluentResults;

namespace Fitness_Tracker_Application.Repository.Workout
{
    public interface IWorkoutRepository
    {
        public Task<Result<Guid>> CreateOrUpdateWorkoutAsync(Guid userId, CreateWorkoutDTO createWorkoutDTO, CancellationToken cancellationToken);

        public Task<Result<ResponseWorkoutDTO>> GetWorkoutById(Guid userId, Guid workoutId, CancellationToken cancellationToken);

        public Task<List<ResponseWorkoutReducedDTO>> GetWorkoutsTimelineAsync(Guid userId, DateTimeOffset? cursor, int limit);
    }
}
