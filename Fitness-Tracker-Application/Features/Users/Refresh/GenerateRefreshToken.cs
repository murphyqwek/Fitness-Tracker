using MediatR;
using System.Security.Cryptography;

namespace Fitness_Tracker_Application.Features.Users.Refresh
{
    public record GenerateRefreshTokenCommand() : IRequest<string>;
    public class GenerateRefreshToken : IRequestHandler<GenerateRefreshTokenCommand, string>
    {
        public Task<string> Handle(GenerateRefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Task.FromResult(Convert.ToBase64String(randomNumber));
            }
        }
    }
}
