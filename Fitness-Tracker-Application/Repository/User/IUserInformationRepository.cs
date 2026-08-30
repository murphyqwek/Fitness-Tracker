using Fitness_Tracker_Application.DTO.User;
using FluentResults;

namespace Fitness_Tracker_Application.Repository.User
{
    public interface IUserInformationRepository
    {
        public Task<Result<UserInformationDTO>> GetUserInformationAsync(Guid id, CancellationToken cancellationToken);

        public Task<Result> UpdateUserInformationAsync(Guid id, UserUpdateDTO userUpdateDTO, CancellationToken cancellationToken);
    }
}
