namespace Fintess_Tracker_Infrastructure.Model
{
    public class MuscleEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public IReadOnlyList<ExerciseMuscleEntity> Exercises { get; set; } = null!;
    }
}
