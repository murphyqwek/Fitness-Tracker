namespace Fitness_Tracker_Shared
{
    public sealed record WorkoutCompletedV1Event(Guid EventId,
                                                 Guid WorkoutId,
                                                 Guid UserId,
                                                 DateTimeOffset CompletedAt,
                                                 IReadOnlyCollection<ExerciseEntry> Exercises);

    public sealed record ExerciseEntry(Guid ExerciseId, IReadOnlyCollection<SetEntry> Sets);

    public sealed record SetEntry(double Weight, int Reps);
}
