namespace Fitness_Tracker_Domain.Entity
{
    public class Workout
    {
        public Guid Id { get; private set; }
        public User User { get; private set; }
        public DateTimeOffset Date { get; private set; }
        public string Description { get; private set; }
        public string Name { get; private set; }
        private List<WorkoutSet> _workoutSets = new List<WorkoutSet>();
        public IReadOnlyList<WorkoutSet> WorkoutSets => _workoutSets.AsReadOnly();

        public Workout(Guid id, User user, DateTimeOffset date, string description, string name)
        {
            Id = id;
            User = user;
            Date = date;
            Description = description;
        }

        public Workout(User user, DateTimeOffset date, List<WorkoutSet> workoutSets, string description, string name) : this(Guid.CreateVersion7(), user, date, description, name)
        {
            _workoutSets = workoutSets;
        }

        public void AddWorkoutSet(WorkoutSet workoutSet)
        {
            _workoutSets.Add(workoutSet);
        }
    }
}
