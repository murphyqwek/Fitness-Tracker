namespace Fitness_Tracker_Infrastructure.Model
{
    public class ExerciseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IReadOnlyList<ExerciseMuscleEntity> Muscles { get; set; } = null!;
    }
}
