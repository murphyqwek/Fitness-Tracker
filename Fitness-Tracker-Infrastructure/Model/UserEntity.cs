namespace Fitness_Tracker_Infrastructure.Model
{
    public class UserEntity
    {
        public Guid Id { get; set; }
        public string Login { get; set; } = null!;
        public string Password { get; set; } = null!;
        public UserInformatonEntity UserInformation { get; set; } = null!;
    }
}
