namespace Fintess_Tracker_Infrastructure.Model
{
    public class WorkoutSetEntity
    {
        public Guid Id { get; set; }
        public Guid WorkoutId { get; set; }
        public WorkoutEntity Workout { get; set; } = null!;
        public ExerciseEntity Exercise { get; set; } = null!;
        public int Repetitions { get; set; }
        public decimal Weight { get; set; }
        public int Order { get; set; }
    }
}
