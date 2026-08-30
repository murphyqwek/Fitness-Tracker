using Fitness_Tracker_Application.DTO.User;
using Fitness_Tracker_Application.Repository.User;
using FluentResults;
using MediatR;

namespace Fitness_Tracker_Application.Features.Users.Infomration
{
    public record GetUserInformationCommand(Guid id) : IRequest<Result<UserInformationDTO>>;
    public class GetUserInformationCommandHandler : IRequestHandler<GetUserInformationCommand, Result<UserInformationDTO>>
    {
        private readonly IUserInformationRepository _repo;

        public GetUserInformationCommandHandler(IUserInformationRepository repo) 
        {
            _repo = repo;
        }

        public async Task<Result<UserInformationDTO>> Handle(GetUserInformationCommand request, CancellationToken cancellationToken)
        {
            return await _repo.GetUserInformationAsync(request.id, cancellationToken);
        }
    }
}
