using Fintess_Tracker_Application.Repository.User;
using Fintess_Tracker_Domain.Entity;
using FluentResults;
using MediatR;

namespace Fintess_Tracker_Application.Features.Users.Registration
{
    public record RegisterUserCommand(string Login, string Password, string Name, DateOnly BirthDay) : IRequest<Result<Guid>>;

    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<Guid>>
    {
        private readonly IUserRepository _userRepository;

        public RegisterUserCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            bool isLoginTaken = await _userRepository.IsLoginAlreadyTakenAsync(request.Login, cancellationToken);
            if (isLoginTaken)
            {
                return Result.Fail<Guid>("User's login is already taken");
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User(request.Login, hashedPassword, request.Name, request.BirthDay);

            await _userRepository.AddNewUserAsync(user, cancellationToken);

            return Result.Ok(user.Id);
        }
    }
}
