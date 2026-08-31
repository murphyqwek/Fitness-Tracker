namespace Fitness_Tracker_Application.DTO.Workout
{
    public record GetWorkoutsResponse(
        List<ResponseWorkoutReducedDTO> Workouts,
        DateTimeOffset? NextCursor
    );

    public record class ResponseWorkoutReducedDTO(Guid Id, string Name, string Description, DateTimeOffset CreatedAt, IList<ResponseWorkoutSetReducedDTO> workoutSets);

    public record class ResponseWorkoutSetReducedDTO(int ExerciseId, string ExerciseName);
}
