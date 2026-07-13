namespace Fitness_Tracker_Infrastructure.Model
{
    public class WorkoutEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public UserEntity User { get; set; } = null!;
        public DateTime Date { get; set; }
        public IReadOnlyList<WorkoutSetEntity> WorkoutSets { get; set; } = null!;
    }
}
