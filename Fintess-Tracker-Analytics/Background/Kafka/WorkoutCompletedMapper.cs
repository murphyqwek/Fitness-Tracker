using AutoMapper;
using Fintess_Tracker_Analytics.DTO;
using Fitness_Tracker_Shared;

namespace Fintess_Tracker_Analytics.Background.Kafka
{
    public class WorkoutCompletedMapper : Profile
    {
        public WorkoutCompletedMapper()
        {
            CreateMap<WorkoutCompletedV1Event, CompletedWorkoutDTO>()
                .ForMember(dest => dest.WorkoutId, opt => opt.MapFrom(src => src.WorkoutId))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.CompletedAt, opt => opt.MapFrom(src => src.CompletedAt))
                .ForMember(dest => dest.Sets, opt => opt.MapFrom(src => src.Exercises.SelectMany(ex => ex.Sets)));
        }
    }
}
