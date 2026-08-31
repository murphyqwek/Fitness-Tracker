namespace Fitness_Tracker_Application.DTO.Workout
{
    public record ResponseWorkoutDTO
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public DateTimeOffset Date { get; init; }
        public IList<ReponseWorkoutSetDTO> WorkoutSets { get; init; } = [];
    }

    public record ReponseWorkoutSetDTO
    {
        public int ExerciseId { get; init; }
        public string ExerciseName { get; init; } = string.Empty;
        public int Repetitions { get; init; }
        public decimal Weight { get; init; }
        public int Order { get; init; }
    }
}
