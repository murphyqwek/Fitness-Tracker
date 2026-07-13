namespace Fitness_Tracker_Domain.Entity
{
    public class WorkoutSet
    {
        public Guid Id { get; private set; }
        public Exercise Exercise { get; private set; }
        public int Repetitions { get; private set; }
        public decimal Weight { get; private set; }
        public int Order { get; private set; }

        public WorkoutSet(Guid id, Exercise exercise, int repetitions, decimal weight, int order)
        {
            Id = id;
            Exercise = exercise;
            Repetitions = repetitions;
            Weight = weight;
            Order = order;
        }

        public WorkoutSet(Exercise exercise, int repetitions, decimal weight, int order) : this(Guid.CreateVersion7(), exercise, repetitions, weight, order)
        {
        }
    }
}
