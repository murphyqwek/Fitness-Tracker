using Fitness_Tracker_Application.DTO.User;
using MediatR;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Fitness_Tracker_Application.Features.Users.JWT
{
    public record GenerateJwtTokenCommand(UserDTO User) : IRequest<string>;
    public class GenerateJwtToken : IRequestHandler<GenerateJwtTokenCommand, string>
    {
        private readonly IJwtSigningCredentialsProvider _signingProvider;

        public GenerateJwtToken(IJwtSigningCredentialsProvider signingProvider)
        {
            _signingProvider = signingProvider;
        }

        public async Task<string> Handle(GenerateJwtTokenCommand request, CancellationToken cancellationToken)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, request.User.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, request.User.Login),
            };

            var token = new JwtSecurityToken(
                issuer: _signingProvider.Issuer,
                audience: _signingProvider.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: _signingProvider.Credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
