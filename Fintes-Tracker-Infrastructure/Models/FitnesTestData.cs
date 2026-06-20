namespace Fintess_Tracker_Infrastructure.Models
{
    public class Fitnes
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Muscle { get; set; } = null!;
        public int Repations { get; set; }

        public int MoodId { get; set; }
    }
}
