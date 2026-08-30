using Fitness_Tracker_Domain.Entity;
using FluentResults;

namespace Fitness_Tracker_Application.Repository.User
{
    public interface IUserRepository
    {
        public Task<Result> AddNewUserAsync(Fitness_Tracker_Domain.Entity.User user, CancellationToken cancellationToken);
        public Task<bool> IsLoginAlreadyTakenAsync(string login, CancellationToken cancellationToken);
        public Task<Result<Fitness_Tracker_Domain.Entity.User>> GetUserByLoginAsync(string login, CancellationToken cancellationToken);
        public Task<Result<Fitness_Tracker_Domain.Entity.User>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken);
    }
}
