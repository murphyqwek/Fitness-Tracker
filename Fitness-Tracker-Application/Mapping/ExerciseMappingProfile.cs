using AutoMapper;
using Fitness_Tracker_Application.DTO.Exercise;
using Fitness_Tracker_Domain.Entity;

namespace Fitness_Tracker_Application.Mapping
{
    public class ExerciseMappingProfile : Profile
    {
        public ExerciseMappingProfile() 
        {
            CreateMap<ExerciseMuscle, ExerciseMuscleDTO>()
                .ForCtorParam(nameof(ExerciseMuscleDTO.Id), opt => opt.MapFrom(src => src.Muscle.Id))
                .ForCtorParam(nameof(ExerciseMuscleDTO.Name), opt => opt.MapFrom(src => src.Muscle.Name));

            CreateMap<Exercise, ExerciseSearchDTO>();
        }
    }
}
