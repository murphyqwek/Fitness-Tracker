using AutoMapper;
using Fitness_Tracker_Application.DTO.User;
using Fitness_Tracker_Application.Repository.User;
using FluentResults;
using MediatR;

namespace Fitness_Tracker_Application.Features.Users
{
    public record GetUserByIdCommand(Guid Id) : IRequest<Result<UserDTO>>;
    public class GetUserById : IRequestHandler<GetUserByIdCommand, Result<UserDTO>>
    {
        private readonly IUserRepository _repository;
        private readonly IMapper _mapper;
        public GetUserById(IUserRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<UserDTO>> Handle(GetUserByIdCommand request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetUserByIdAsync(request.Id, cancellationToken);

            if(result.IsFailed)
            {
                return Result.Fail<UserDTO>(result.Errors);
            }

            var userDTO = _mapper.Map<UserDTO>(result.Value);

            return Result.Ok(userDTO);
        }
    }
}
