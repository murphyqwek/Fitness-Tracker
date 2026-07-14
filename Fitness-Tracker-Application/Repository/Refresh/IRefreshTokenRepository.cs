using FluentResults;

namespace Fitness_Tracker_Application.Repository.Refresh
{
    public interface IRefreshTokenRepository
    {
        public Task AddNewRefreshTokenAsync(string refreshToken, Guid userId, TimeSpan expiry, CancellationToken cancellationToken);
        public Task RemoveRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
        public Task<Result<Guid>> CheckRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
    }
}
