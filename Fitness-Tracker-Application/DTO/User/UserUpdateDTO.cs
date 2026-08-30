namespace Fitness_Tracker_Application.DTO.User
{
    public record class UserUpdateDTO(string? name, DateOnly? birthDay, int? height, decimal? weight)
    {
    }
}
