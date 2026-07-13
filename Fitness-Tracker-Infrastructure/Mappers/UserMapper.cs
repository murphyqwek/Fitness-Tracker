using Fitness_Tracker_Domain.Entity;
using Fitness_Tracker_Infrastructure.Model;

namespace Fitness_Tracker_Infrastructure.Mappers
{
    public class UserMapper
    {
        public static User MapToDomain(UserEntity userEntity)
        {
            return new User(
                id: userEntity.Id,
                login: userEntity.Login,
                password: userEntity.Password,
                name: userEntity.Name,
                birthDay: userEntity.BirthDay
            );
        }

        public static UserEntity MapToEntity(User user)
        {
            return new UserEntity
            {
                Id = user.Id,
                Login = user.Login,
                Password = user.Password,
                Name = user.Name,
                BirthDay = user.BirthDay
            };
        }
    }
}
