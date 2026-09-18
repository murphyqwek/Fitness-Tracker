using Microsoft.IdentityModel.Tokens;
namespace Fitness_Tracker_Application.Features.Users.JWT
{
    public interface IJwtSigningCredentialsProvider
    {
        SigningCredentials Credentials { get; }
        string Issuer { get; }
        string Audience { get; }
    }
}
