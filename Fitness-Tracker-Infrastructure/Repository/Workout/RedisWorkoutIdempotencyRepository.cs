using Fitness_Tracker_Application.Repository.Workout;
using StackExchange.Redis;

namespace Fitness_Tracker_Infrastructure.Repository.Workout
{
    public class RedisWorkoutIdempotencyRepository : IWorkoutIdempotencyKeyRepository
    {
        private readonly IDatabase _cache;
        private const int MAX_RETRIES = 3;

        public RedisWorkoutIdempotencyRepository(IConnectionMultiplexer connectionMultiplexer) 
        {
            _cache = connectionMultiplexer.GetDatabase();
        }

        private string GetKey(Guid userId, Guid idempotencyKey) 
        {
            return $"workout_idempotency:{userId}:{idempotencyKey}";
        }

        public async Task<(IdempotencyStatus Status, Guid? Result)> LockWorkout(Guid userId, Guid idempotencyKey)
        {
            string key = GetKey(userId, idempotencyKey);
            for (int i = 0; i < MAX_RETRIES; i++)
            {
                var result = await TryLock(key);

                if(result.Status != IdempotencyStatus.UNKNOW) 
                {
                    return result;
                }
            }

            return (IdempotencyStatus.UNKNOW, null);
        }

        private async Task<(IdempotencyStatus Status, Guid? Result)> TryLock(string key) 
        {
            bool setResult = await _cache.StringSetAsync(key, "RUNNING", TimeSpan.FromHours(5), When.NotExists);

            if (setResult)
            {
                return (IdempotencyStatus.TAKEN, null);
            }

            string? status = await _cache.StringGetAsync(key);

            if (status == null)
            {
                return (IdempotencyStatus.UNKNOW, null);
            }

            if(status == "RUNNING") 
            {
                return (IdempotencyStatus.RUNNING, null); 
            }

            return (IdempotencyStatus.FINISHED, new Guid(status));
        }

        public async Task UpdateWorkoutIdempotencyStatus(Guid userId, Guid idempotencyKey, Guid workoutId)
        {
            string key = GetKey(userId, idempotencyKey);
            await _cache.StringSetAsync(key, workoutId.ToString(), TimeSpan.FromHours(5), When.Exists);
        }

        public async Task DeleteWorkoutIdempotencyStatus(Guid userId, Guid idempotencyKey)
        {
            string key = GetKey(userId, idempotencyKey);

            await _cache.KeyDeleteAsync(key);
        }
    }
}
