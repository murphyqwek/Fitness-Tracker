using Fitness_Tracker_Application.DTO.User;
using Fitness_Tracker_Application.Mappers;
using Fitness_Tracker_Application.Repository.User;
using FluentResults;
using MediatR;

namespace Fitness_Tracker_Application.Features.Users
{
    public record GetUserByIdCommand(Guid Id) : IRequest<Result<UserDTO>>;
    public class GetUserById : IRequestHandler<GetUserByIdCommand, Result<UserDTO>>
    {
        private readonly IUserRepository _repository;

        public GetUserById(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<UserDTO>> Handle(GetUserByIdCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetUserByIdAsync(request.Id, cancellationToken);

            if(result.IsFailed)
            {
                return Result.Fail<UserDTO>(result.Errors);
            }

            var userDTO = UserDTOMapper.MapToDTO(result.Value);

            return Result.Ok(userDTO);
        }
    }
}
