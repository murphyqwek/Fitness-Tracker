using Fitness_Tracker_Application.DTO.User;
using Fitness_Tracker_Application.Repository.User;
using FluentResults;
using MediatR;

namespace Fitness_Tracker_Application.Features.Users.Infomration
{
    public record UpdateUserInformationCommand(Guid id, UserUpdateDTO userUpdateDto) : IRequest<Result>;
    public class UpdateUserInformationCommandHandler : IRequestHandler<UpdateUserInformationCommand, Result>
    {
        private readonly IUserInformationRepository _repo;

        public UpdateUserInformationCommandHandler(IUserInformationRepository repo)
        {
            _repo = repo;
        }

        public async Task<Result> Handle(UpdateUserInformationCommand request, CancellationToken cancellationToken)
        {
            return await _repo.UpdateUserInformationAsync(request.id, request.userUpdateDto, cancellationToken);
        }
    }
}