namespace Fitness_Tracker_Domain.Entity
{
    public class UserInformation
    {
        public Guid Id { get; private set; }
        public User User { get; private set; }
        public string? Name { get; private set; }
        public DateOnly? BirthDay { get; private set; }
        public int? Height { get; private set; }
        public decimal? Weight { get; private set; }

        public UserInformation(Guid id, User user, string? name, DateOnly? birthDay, int? height, decimal? weight)
        {
            Id = id;
            User = user;
            Name = name;
            BirthDay = birthDay;
            Height = height;
            Weight = weight;
        }
    }
}
