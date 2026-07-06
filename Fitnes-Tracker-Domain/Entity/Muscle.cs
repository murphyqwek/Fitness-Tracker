namespace Fintess_Tracker_Domain.Entity
{
    public class Muscle
    {
        public int Id { get; private set; }
        public string Name { get; private set; }

        public Muscle(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
