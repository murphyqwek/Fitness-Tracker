using Fitness_Tracker_Application.DTO.Workout;
using Fitness_Tracker_Application.Repository.Workout;
using MediatR;

namespace Fitness_Tracker_Application.Features.Workout
{
    public record GetWorkoutsQuery(Guid UserId, DateTimeOffset? Cursor, int Limit = 10)
    : IRequest<GetWorkoutsResponse>;

    public class GetWorkoutsQueryHandler : IRequestHandler<GetWorkoutsQuery, GetWorkoutsResponse>
    {
        private readonly IWorkoutRepository _repo;

        public GetWorkoutsQueryHandler(IWorkoutRepository repo)
        {
            _repo = repo;
        }

        public async Task<GetWorkoutsResponse> Handle(GetWorkoutsQuery request, CancellationToken cancellationToken)
        {
            int safeLimit = Math.Min(request.Limit, 50);

            var workouts = await _repo.GetWorkoutsTimelineAsync(request.UserId, request.Cursor, safeLimit);

            DateTimeOffset? nextCursor = workouts.Count == safeLimit
                ? workouts.Last().CreatedAt
                : null;

            return new GetWorkoutsResponse(workouts, nextCursor);
        }
    }
}
