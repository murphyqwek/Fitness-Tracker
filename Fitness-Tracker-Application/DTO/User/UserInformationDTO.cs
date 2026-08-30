namespace Fitness_Tracker_Application.DTO.User
{
    public record class UserInformationDTO(string login, string? name, DateOnly? birthDay, int? height, decimal? weight)
    {
    }
}
