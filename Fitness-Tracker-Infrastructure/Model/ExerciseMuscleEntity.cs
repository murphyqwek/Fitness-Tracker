namespace Fitness_Tracker_Infrastructure.Model
{
    public class ExerciseMuscleEntity
    {
        public int ExerciseId { get; set; }
        public ExerciseEntity Exercise { get; set; } = null!;

        public int MuscleId { get; set; }

        public MuscleEntity Muscle { get; set; }
        public decimal PercentageOfUsage { get; set; }
    }
}
