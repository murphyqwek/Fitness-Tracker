using AutoMapper;
using Fintess_Tracker_Analytics.DTO;
using Fitness_Tracker_Shared;

public class WorkoutCompletedMapper : Profile
{
    public WorkoutCompletedMapper()
    {
        CreateMap<SetEntry, CompletedSetDTO>()
            .ForCtorParam(
                nameof(CompletedSetDTO.Weight),
                opt => opt.MapFrom(src => src.Weight))
            .ForCtorParam(
                nameof(CompletedSetDTO.Repetitions),
                opt => opt.MapFrom(src => src.Reps));

        CreateMap<WorkoutCompletedV1Event, CompletedWorkoutDTO>()
            .ForCtorParam(
                nameof(CompletedWorkoutDTO.WorkoutId),
                opt => opt.MapFrom(src => src.WorkoutId))
            .ForCtorParam(
                nameof(CompletedWorkoutDTO.UserId),
                opt => opt.MapFrom(src => src.UserId))
            .ForCtorParam(
                nameof(CompletedWorkoutDTO.CompletedAt),
                opt => opt.MapFrom(src => src.CompletedAt))
            .ForCtorParam(
                nameof(CompletedWorkoutDTO.Sets),
                opt => opt.MapFrom(src =>
                    src.Exercises.SelectMany(ex => ex.Sets)));
    }
}