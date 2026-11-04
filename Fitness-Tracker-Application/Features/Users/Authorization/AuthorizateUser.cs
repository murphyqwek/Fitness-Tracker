using Fitness_Tracker_Application.Repository.User;
using Fitness_Tracker_Application.DTO.User;
using FluentResults;
using MediatR;
using AutoMapper;
using Fitness_Tracker_Application.Service.Metrics;

namespace Fitness_Tracker_Application.Features.Users.Authorization
{
    public record AuthorizateUserCommand(string Login, string Password) : IRequest<Result<UserDTO>>;

    public class AuthorizateUserCommandHandler : IRequestHandler<AuthorizateUserCommand, Result<UserDTO>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IFitnessMetrics _metrics;
        public AuthorizateUserCommandHandler(IUserRepository userRepository, IMapper mapper, IFitnessMetrics metrics)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _metrics = metrics;
        }

        public async Task<Result<UserDTO>> Handle(AuthorizateUserCommand request, CancellationToken cancellationToken)
        {
            var result = await _userRepository.GetUserByLoginAsync(request.Login, cancellationToken);

            if (result.IsFailed)
            {
                _metrics.RecordLoginAttempt(false);
                return Result.Fail<UserDTO>("User not found");
            }

            bool isVerified = BCrypt.Net.BCrypt.Verify(request.Password, result.Value.Password);

            if (isVerified)
            {
                var userDTO = _mapper.Map<UserDTO>(result.Value);
                _metrics.RecordLoginAttempt(true);
                return Result.Ok(userDTO);
            }
            else
            {
                _metrics.RecordLoginAttempt(false);
                return Result.Fail<UserDTO>("Invalid password");
            }
        }
    }
}
