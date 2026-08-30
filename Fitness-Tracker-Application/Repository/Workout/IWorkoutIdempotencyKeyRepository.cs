namespace Fitness_Tracker_Application.Repository.Workout
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
        public Task<(IdempotencyStatus Status, Guid? Result)> LockWorkout(Guid userId, Guid idempotencyKey);

        public Task UpdateWorkoutIdempotencyStatus(Guid userId, Guid idempotencyKey, Guid exerciseId);

        public Task DeleteWorkoutIdempotencyStatus(Guid userId, Guid idempotencyKey);
    }
}
