using Fintess_Tracker_Domain.Entity;
using FluentResults;

namespace Fintess_Tracker_Application.Repository.User
{
    public interface IUserRepository
    {
        public Task AddNewUserAsync(Fintess_Tracker_Domain.Entity.User user, CancellationToken cancellationToken);
        public Task<bool> IsLoginAlreadyTakenAsync(string login, CancellationToken cancellationToken);
        public Task<Result<Fintess_Tracker_Domain.Entity.User>> GetUserByLoginAsync(string login, CancellationToken cancellationToken);
    }
}
