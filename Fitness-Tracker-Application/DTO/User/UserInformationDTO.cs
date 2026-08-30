namespace Fitness_Tracker_Application.DTO.User
{
    public class UserInformationDTO
    {
        public UserInformationDTO() { }
        
        public UserInformationDTO(string login, string? name, DateOnly? birthDay, int? height, decimal? weight) : this()
        {
            Login = login;
            Name = name;
            BirthDay = birthDay;
            Height = height;
            Height = height;
            Weight = weight;
        }

        public string? Login { get; set; }
        public string? Name { get; set; }
        public DateOnly? BirthDay { get; set; }
        public int? Height { get; set; }
        public decimal? Weight { get; set; }
    }
}
