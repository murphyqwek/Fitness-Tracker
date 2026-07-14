using Fitness_Tracker_Application.Repository.Refresh;
using FluentResults;
using MediatR;

namespace Fitness_Tracker_Application.Features.Users.Refresh
{
    public record CheckRefreshTokenCommand(string RefreshToken) : IRequest<Result<Guid>>;
    public class CheckRefreshToken : IRequestHandler<CheckRefreshTokenCommand, Result<Guid>>
    {
        private readonly IRefreshTokenRepository _repository;
        public CheckRefreshToken(IRefreshTokenRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<Guid>> Handle(CheckRefreshTokenCommand request, CancellationToken cancellationToken)
        {
            return await _repository.CheckRefreshTokenAsync(request.RefreshToken, cancellationToken);
        }
    }
}
