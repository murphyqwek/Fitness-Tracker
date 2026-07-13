using Fitness_Tracker_Application.Repository.User;
using Fitness_Tracker_Application.DTO.User;
using FluentResults;
using MediatR;

namespace Fitness_Tracker_Application.Features.Users.Authorization
{
    public record AuthorizateUserCommand(string Login, string Password) : IRequest<Result<UserDTO>>;

    public class AuthorizateUserCommandHandler : IRequestHandler<AuthorizateUserCommand, Result<UserDTO>>
    {
        private readonly IUserRepository _userRepository;

        public AuthorizateUserCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result<UserDTO>> Handle(AuthorizateUserCommand request, CancellationToken cancellationToken)
        {
            var result = await _userRepository.GetUserByLoginAsync(request.Login, cancellationToken);

            if (result.IsFailed)
            {
                return Result.Fail<UserDTO>("User not found");
            }

            bool isVerified = BCrypt.Net.BCrypt.Verify(request.Password, result.Value.Password);

            if (isVerified)
            {
                var userDTO = new UserDTO(result.Value.Login, result.Value.Name, result.Value.BirthDay, result.Value.Id);
                return Result.Ok(userDTO);
            }
            else
            {
                return Result.Fail<UserDTO>("Invalid password");
            }
        }
    }
}
