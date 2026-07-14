using Fitness_Tracker_Application.Repository.Refresh;
using FluentResults;
using StackExchange.Redis;

namespace Fitness_Tracker_Infrastructure.Repository.Refresh
{
    public class RedisRefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly IDatabase _database;

        public RedisRefreshTokenRepository(IConnectionMultiplexer connection)
        {
            _database = connection.GetDatabase();
        }

        public async Task AddNewRefreshTokenAsync(string refreshToken, Guid userId, TimeSpan expiry, CancellationToken cancellationToken)
        {
            await _database.StringSetAsync(refreshToken, userId.ToString(), expiry);
        }

        public async Task<Result<Guid>> CheckRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
        {
            var userIdString = await _database.StringGetAsync(refreshToken);

            if(userIdString.IsNullOrEmpty)
            {
                return Result.Fail<Guid>("Refresh token was expired or not found");
            }

            Guid result;
            if (Guid.TryParse(userIdString.ToString(), out result))
            {
                return Result.Ok(result);
            }

            return Result.Fail<Guid>("Invalid user ID format in refresh token");
        }

        public async Task RemoveRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
        {
            await _database.KeyDeleteAsync(refreshToken);
        }
    }
}
