using AutoMapper;
using Fitness_Tracker_Application.DTO.Workout;
using Fitness_Tracker_Infrastructure.Model;

namespace Fitness_Tracker_Infrastructure.Mapping
{
    public class WorkoutMapping : Profile
    {
        public WorkoutMapping()
        {
            CreateMap<CreateWorkoutSetDTO, WorkoutSetEntity>()
                .ForMember(dist => dist.Id, opt => opt.MapFrom(_ => Guid.CreateVersion7()))
                .ForMember(dist => dist.Workout, opt => opt.Ignore())
                .ForMember(dist => dist.Exercise, opt => opt.Ignore());

            CreateMap<CreateWorkoutDTO, WorkoutEntity>()
                .ForMember(dist => dist.Id, opt => opt.MapFrom(_ => Guid.CreateVersion7()))
                .ForMember(dest => dest.WorkoutSets, opt => opt.MapFrom(src => src.workoutSets))
                .ForMember(dist => dist.UserId, opt => opt.Ignore())
                .ForMember(dist => dist.User, opt => opt.Ignore());


            CreateMap<WorkoutSetEntity, ReponseWorkoutSetDTO>()
                .ForMember(dist => dist.ExerciseName, opt => opt.MapFrom(src => src.Exercise.Name));

            CreateMap<WorkoutEntity, ResponseWorkoutDTO>();

            CreateMap<ExerciseEntity, ResponseWorkoutSetReducedDTO>()
            .ConstructUsing(e => new ResponseWorkoutSetReducedDTO(e.Id, e.Name));

            CreateMap<WorkoutSetEntity, ResponseWorkoutSetReducedDTO>()
                .ConstructUsing(s => new ResponseWorkoutSetReducedDTO(s.ExerciseId, s.Exercise.Name));

            CreateMap<WorkoutEntity, ResponseWorkoutReducedDTO>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.UtcDateTime))
                .ForMember(dest => dest.workoutSets, opt => opt.MapFrom(src =>
                    src.WorkoutSets
                       .Select(s => s.Exercise) 
                       .Distinct()
                ));
        }
    }
}
