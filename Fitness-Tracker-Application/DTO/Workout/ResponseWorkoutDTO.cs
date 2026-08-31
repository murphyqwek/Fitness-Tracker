namespace Fitness_Tracker_Application.DTO.Workout
{
    public record class ResponseWorkoutDTO(Guid Id, string Name, string Description, DateTimeOffset Date, IList<ReponseWorkoutSetDTO> workoutSets);

    public record class ReponseWorkoutSetDTO(int ExerciseId, string ExerciseName, int Repetitions, decimal Weight, int Order);
}
