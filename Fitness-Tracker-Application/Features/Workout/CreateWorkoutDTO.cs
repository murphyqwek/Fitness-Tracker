namespace Fitness_Tracker_Application.Features.Workout
{
    public record class WorkoutRequestDTO(Guid UserId, Guid IdempotencyKey, CreateWorkoutDTO Workout);

    public record class CreateWorkoutDTO(string Name, string Description, DateTime Date, IList<CreateWorkoutSetDTO> workoutSets);

    public record class CreateWorkoutSetDTO(int ExerciseId, int Repetitions, decimal Weight, int Order);
}
