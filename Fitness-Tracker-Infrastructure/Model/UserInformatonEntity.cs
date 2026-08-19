using Fitness_Tracker_Domain.Entity;

namespace Fitness_Tracker_Infrastructure.Model
{
    public class UserInformatonEntity
    {
        public Guid Id { get; set; }
        public UserEntity User { get; set; } = null!;
        public string? Name { get; set; }
        public DateOnly? BirthDay { get; set; }
        public int? Height { get; set; }
        public decimal? Weight { get; set; }
    }
}
