namespace Fintess_Tracker_Domain.Entity
{
    public class Workout
    {
        public Guid Id { get; private set; }
        public User User { get; private set; }
        public DateTime Date { get; private set; }

        private List<WorkoutSet> _workoutSets = new List<WorkoutSet>();
        public IReadOnlyList<WorkoutSet> WorkoutSets => _workoutSets.AsReadOnly();

        public Workout(Guid id, User user, DateTime date)
        {
            Id = id;
            User = user;
            Date = date;
        }

        public Workout(User user, DateTime date, List<WorkoutSet> workoutSets) : this(Guid.NewGuid(), user, date)
        {
            _workoutSets = workoutSets;
        }

        public void AddWorkoutSet(WorkoutSet workoutSet)
        {
            _workoutSets.Add(workoutSet);
        }
    }
}
