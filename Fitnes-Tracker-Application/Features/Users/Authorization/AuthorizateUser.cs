using Fintess_Tracker_Application.Repository.User;
using MediatR;

namespace Fintess_Tracker_Application.Features.Users.Authorization
{
    public record AuthorizateUserCommand(string Login, string Password) : IRequest<Guid>;

    public class AuthorizateUserCommandHandler : IRequestHandler<AuthorizateUserCommand, Guid>
    {
        private readonly IUserRepository _userRepository;

        public AuthorizateUserCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Guid> Handle(AuthorizateUserCommand request, CancellationToken cancellationToken)
        {
            var result = await _userRepository.GetUserByLoginAsync(request.Login, cancellationToken);

            if (result.IsFailed)
            {
                throw new Exception("User was not found");
            }

            bool isVerified =BCrypt.Net.BCrypt.Verify(request.Password, result.Value.Password);

            if (isVerified)
            {
                return result.Value.Id;
            }
            else
            {
                throw new Exception("Password is incorrect");
            }
        }
    }
}
