namespace Fintess_Tracker_Analytics.DTO
{
    public sealed record CompletedWorkoutDTO(Guid WorkoutId, Guid UserId, DateTimeOffset CompletedAt, IReadOnlyCollection<CompletedSetDTO> Sets);

    public sealed record CompletedSetDTO(decimal Weight, int Repetitions);
}
