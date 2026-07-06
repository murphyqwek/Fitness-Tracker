namespace Fintess_Tracker_Domain.Entity
{
    public class Exercise
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public IReadOnlyList<ExerciseMuscle> Muscles { get; private set; }

        public Exercise(int id, string name, string description, IReadOnlyList<ExerciseMuscle> muscles)
        {
            Id = id;
            Name = name;
            Description = description;
            Muscles = muscles;
        }
    }
}
