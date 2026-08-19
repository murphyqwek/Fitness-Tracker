using AutoMapper;
using Fitness_Tracker_Application.DTO.User;

namespace Fitness_Tracker_Application.Mapping
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile() 
        { 
            CreateMap<Fitness_Tracker_Domain.Entity.User, UserDTO>()
                .ForMember(dest => dest.Login, opt => opt.MapFrom(src => src.Login))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));
        }
    }
}
