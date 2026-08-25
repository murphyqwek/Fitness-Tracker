using AutoMapper;
using Fitness_Tracker_Domain.Entity;
using Fitness_Tracker_Infrastructure.Model;

namespace Fitness_Tracker_Infrastructure.Mapping
{
    public class ExerciseEntityMapping : Profile
    {
        public ExerciseEntityMapping() 
        {
            CreateMap<Muscle, MuscleEntity>().ReverseMap();

            CreateMap<ExerciseMuscle, ExerciseMuscleEntity>().ReverseMap();

            CreateMap<Exercise, ExerciseEntity>().ReverseMap();
        }
    }
}
