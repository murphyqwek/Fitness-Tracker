namespace Fitness_Tracker_Application.DTO.Workout
{
    public record class ResponseWorkoutReducedDTO(Guid Id, string Name, string Description, DateTime Date, IList<ResponseWorkoutSetReducedDTO> workoutSets);

    public record class ResponseWorkoutSetReducedDTO(int ExerciseId, string ExerciseName);
}
