using AutoMapper;
using Fitness_Tracker_Domain.Entity;

namespace Fitness_Tracker_Infrastructure.Mapping
{
    public class UserEntityMapping : Profile
    {
        public UserEntityMapping() 
        {
            CreateMap<User, Model.UserEntity>().ReverseMap();
        }
    }
}
