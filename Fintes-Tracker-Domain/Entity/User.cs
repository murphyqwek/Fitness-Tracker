namespace Fintess_Tracker_Domain.Entity
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Login { get; private set; }
        public string Password { get; private set; }

        public string Name { get; private set; }
        public DateOnly? BirthDay { get; private set; }

        public User(Guid id, string login, string password, string name, DateOnly? birthDay)
        {
            Id = id;
            Login = login;
            Password = password;
            Name = name;
            BirthDay = birthDay;
        }

        public User(string login, string password, string name, DateOnly? birthDay) : this(Guid.NewGuid(), login, password, name, birthDay)
        {
        }
    }
}
