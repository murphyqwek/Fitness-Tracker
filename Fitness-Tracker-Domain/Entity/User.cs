namespace Fitness_Tracker_Domain.Entity
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Login { get; private set; }
        public string Password { get; private set; }

        public User(Guid id, string login, string password)
        {
            Id = id;
            Login = login;
            Password = password;
        }

        public User(string login, string password) : this(Guid.CreateVersion7(), login, password)
        {
        }
    }
}
