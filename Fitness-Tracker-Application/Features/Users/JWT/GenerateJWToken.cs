using Fitness_Tracker_Application.DTO.User;
using MediatR;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Fitness_Tracker_Application.Features.Users.JWT
{
    public record GenerateJwtTokenCommand(UserDTO User) : IRequest<string>;
    public class GenerateJwtToken : IRequestHandler<GenerateJwtTokenCommand, string>
    {
        private readonly JwtConfigDTO _configuration;

        public GenerateJwtToken(IOptions<JwtConfigDTO> configuration)
        {
            _configuration = configuration.Value;
        }

        public async Task<string> Handle(GenerateJwtTokenCommand request, CancellationToken cancellationToken)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.Key));
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            List<Claim> claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, request.User.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, request.User.Login),
            };

            var token = new JwtSecurityToken(
                issuer: _configuration.Issuer,
                audience: _configuration.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
