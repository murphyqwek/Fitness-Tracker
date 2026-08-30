using AutoMapper;
using Fitness_Tracker_Application.DTO.User;
using Fitness_Tracker_Infrastructure.Model;

namespace Fitness_Tracker_Infrastructure.Mapping
{
    public class UserInformationMapping : Profile
    {
        public UserInformationMapping() 
        {
            CreateMap<UserInformatonEntity, UserInformationDTO>()
                .ForMember(dest => dest.login, opt => opt.MapFrom(src => src.User.Login));
        }
    }
}
