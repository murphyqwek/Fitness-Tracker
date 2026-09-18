namespace Fitness_Tracker_Application.Features.Users.JWT
{
    public class JwtConfigDTO
    {
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string PublicKeyPath { get; set; } = string.Empty;
        public string PrivateKeyPath { get; set; } = string.Empty;
    }
}
