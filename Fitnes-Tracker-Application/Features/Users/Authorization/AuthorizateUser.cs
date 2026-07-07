using Fintess_Tracker_Application.Repository.User;
using FluentResults;
using MediatR;

namespace Fintess_Tracker_Application.Features.Users.Authorization
{
    public record AuthorizateUserCommand(string Login, string Password) : IRequest<Result<Guid>>;

    public class AuthorizateUserCommandHandler : IRequestHandler<AuthorizateUserCommand, Result<Guid>>
    {
        private readonly IUserRepository _userRepository;

        public AuthorizateUserCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result<Guid>> Handle(AuthorizateUserCommand request, CancellationToken cancellationToken)
        {
            var result = await _userRepository.GetUserByLoginAsync(request.Login, cancellationToken);

            if (result.IsFailed)
            {
                return Result.Fail<Guid>("User not found");
            }

            bool isVerified =BCrypt.Net.BCrypt.Verify(request.Password, result.Value.Password);

            if (isVerified)
            {
                return Result.Ok(result.Value.Id);
            }
            else
            {
                return Result.Fail<Guid>("Invalid password");
            }
        }
    }
}
