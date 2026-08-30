using FluentResults;

namespace Fitness_Tracker_Application.Features.Workout
{
    public interface IWorkoutRepository
    {
        public Task<Result<Guid>> CreateNewWorkout(Guid userId, CreateWorkoutDTO createWorkoutDTO);

        public Task<Result<ResponseWorkoutDTO>> GetWorkoutById(Guid userId, Guid workoutId);
    }
}
