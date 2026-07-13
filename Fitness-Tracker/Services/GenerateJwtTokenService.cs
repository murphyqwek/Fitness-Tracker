using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Fitness_Tracker_Application.DTO.User;
using Fitness_Tracker.DTO.Configuration;
using Microsoft.Extensions.Options;
namespace Fitness_Tracker.Services
{
    public class GenerateJwtTokenService
    {
        private readonly JwtConfigDTO _configuration;

        public GenerateJwtTokenService(IOptions<JwtConfigDTO> configuration)
        {
            _configuration = configuration.Value;
        }

        public string Generate(UserDTO user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.Key));
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            List<Claim> claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Login),
                new Claim(JwtRegisteredClaimNames.Name, user.Name),
            };

            var token = new JwtSecurityToken(
                issuer: _configuration.Issuer,
                audience: _configuration.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(15),
                signingCredentials: signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
