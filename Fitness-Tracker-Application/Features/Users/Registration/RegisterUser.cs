using AutoMapper;
using Fitness_Tracker_Application.DTO.User;
using Fitness_Tracker_Application.Repository.User;
using Fitness_Tracker_Domain.Entity;
using FluentResults;
using MediatR;

namespace Fitness_Tracker_Application.Features.Users.Registration
{
    public record RegisterUserCommand(string Login, string Password) : IRequest<Result<UserDTO>>;

    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<UserDTO>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public RegisterUserCommandHandler(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<Result<UserDTO>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            bool isLoginTaken = await _userRepository.IsLoginAlreadyTakenAsync(request.Login, cancellationToken);
            if (isLoginTaken)
            {
                return Result.Fail($"User's login {request.Login} is already taken");
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User(request.Login, hashedPassword);

            var result = await _userRepository.AddNewUserAsync(user, cancellationToken);

            if(result.IsFailed) {
                return result;
            }

            return Result.Ok(_mapper.Map<UserDTO>(user));
        }
    }
}
