using Fitness_Tracker_Application.DTO.User;
using Fitness_Tracker_Application.Mappers;
using Fitness_Tracker_Application.Repository.User;
using Fitness_Tracker_Domain.Entity;
using FluentResults;
using MediatR;

namespace Fitness_Tracker_Application.Features.Users.Registration
{
    public record RegisterUserCommand(string Login, string Password, string Name, DateOnly BirthDay) : IRequest<Result<UserDTO>>;

    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<UserDTO>>
    {
        private readonly IUserRepository _userRepository;

        public RegisterUserCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result<UserDTO>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            bool isLoginTaken = await _userRepository.IsLoginAlreadyTakenAsync(request.Login, cancellationToken);
            if (isLoginTaken)
            {
                return Result.Fail<UserDTO>("User's login is already taken");
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User(request.Login, hashedPassword, request.Name, request.BirthDay);

            await _userRepository.AddNewUserAsync(user, cancellationToken);

            return Result.Ok(UserDTOMapper.MapToDTO(user));
        }
    }
}
