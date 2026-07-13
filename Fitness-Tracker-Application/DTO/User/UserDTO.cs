namespace Fitness_Tracker_Application.DTO.User
{
    public record UserDTO(string Login, string Name, DateOnly? BirthDay, Guid Id)
    {
    }
}
