using Fitness_Tracker_Application.DTO.User;
using MediatR;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Fitness_Tracker_Application.Features.Users.JWT
{
    public record GenerateJwtTokenCommand(UserDTO User) : IRequest<string>;
    public class GenerateJwtToken : IRequestHandler<GenerateJwtTokenCommand, string>, IDisposable
    {
        private readonly JwtConfigDTO _configuration;
        private readonly SigningCredentials _signingCredentials;
        private readonly RSA _rsaKey;

        public GenerateJwtToken(IOptions<JwtConfigDTO> configuration)
        {
            _configuration = configuration.Value;
            string privatePem = File.ReadAllText(_configuration.PrivateKeyPath);

            _rsaKey = RSA.Create();
            _rsaKey.ImportFromPem(privatePem);
            var securityKey = new RsaSecurityKey(_rsaKey);

            _signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);
        }

        public async Task<string> Handle(GenerateJwtTokenCommand request, CancellationToken cancellationToken)
        {
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
                signingCredentials: _signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public void Dispose()
        {
            _rsaKey?.Dispose();
        }
    }
}
