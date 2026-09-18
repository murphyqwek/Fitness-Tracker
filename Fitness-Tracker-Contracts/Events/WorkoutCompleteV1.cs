namespace Fitness_Tracker_Contracts.Events
{
    public record class WorkoutCompleteV1(Guid EventId, Guid UserId, Guid WorkoutId, DateTimeOffset CompletedAt, IReadOnlyList<CompletedExerciseV1> Exercises);

    public sealed record CompletedExerciseV1(int ExerciseId, IReadOnlyList<CompletedSetV1> Sets);

    public sealed record CompletedSetV1(decimal Weight, int Repetitions);
}
