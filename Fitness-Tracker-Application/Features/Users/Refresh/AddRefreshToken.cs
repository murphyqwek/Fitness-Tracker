using Fitness_Tracker_Application.Repository.Refresh;
using MediatR;

namespace Fitness_Tracker_Application.Features.Users.Refresh
{
    public record AddRefreshTokenCommand(string RefreshToken, Guid id, TimeSpan expire) : IRequest;
    public class AddRefreshToken : IRequestHandler<AddRefreshTokenCommand>
    {
        private readonly IRefreshTokenRepository _repository;

        public AddRefreshToken(IRefreshTokenRepository repository)
        {
            _repository = repository;
        }

        public async Task Handle(AddRefreshTokenCommand request, CancellationToken cancellationToken)
        {
            await _repository.AddNewRefreshTokenAsync(request.RefreshToken, request.id, request.expire, cancellationToken);
        }
    }
}
