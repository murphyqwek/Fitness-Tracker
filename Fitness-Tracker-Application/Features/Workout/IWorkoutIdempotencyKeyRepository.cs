namespace Fitness_Tracker_Application.Features.Workout
{
    public enum IdempotencyStatus 
    {
        RUNNING,
        FINISHED,
        TAKEN,
        UNKNOW,
    }

    public interface IWorkoutIdempotencyKeyRepository
    {
        public Task<(IdempotencyStatus Status, string? Result)> LockWorkout(Guid userId, Guid idempotencyKey);

        public Task UpdateWorkoutIdempotencyStatus(Guid userId, Guid idempotencyKey, Guid exerciseId);

        public Task DeleteWorkoutIdempotencyStatus(Guid userId, Guid idempotencyKey);
    }
}
