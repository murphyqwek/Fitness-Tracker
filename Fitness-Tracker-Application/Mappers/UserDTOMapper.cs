using Fitness_Tracker_Application.DTO.User;
using Fitness_Tracker_Domain.Entity;

namespace Fitness_Tracker_Application.Mappers
{
    public static class UserDTOMapper
    {
        public static UserDTO MapToDTO(User user)
        {
            return new UserDTO(
                user.Login,
                user.Id);
        }
    }
}
