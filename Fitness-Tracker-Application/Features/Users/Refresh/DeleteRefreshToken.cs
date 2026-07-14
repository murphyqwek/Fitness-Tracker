using Fitness_Tracker_Application.Repository.Refresh;
using MediatR;

namespace Fitness_Tracker_Application.Features.Users.Refresh
{
    public record DeleteRefreshTokenCommand(string RefreshToken) : IRequest;
    public class DeleteRefreshToken : IRequestHandler<DeleteRefreshTokenCommand>
    {
        private readonly IRefreshTokenRepository _repository;

        public DeleteRefreshToken(IRefreshTokenRepository repository)
        {
            _repository = repository;
        }

        public async Task Handle(DeleteRefreshTokenCommand request, CancellationToken cancellationToken)
        {
            await _repository.RemoveRefreshTokenAsync(request.RefreshToken, cancellationToken);
        }
    }
}
