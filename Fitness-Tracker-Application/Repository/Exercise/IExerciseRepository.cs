using Fitness_Tracker_Application.Features.Exercise;
using Fitness_Tracker_Application.Service.Pagination;
using FluentResults;

namespace Fitness_Tracker_Application.Repository.Exercises
{
    public interface IExerciseRepository
    {
        public Task<Result<ExerciseSearchDTO>> GetExerciseByIdAsync(int id, CancellationToken cancellationToken);

        public Task<PaginationResponse<ExerciseSearchDTO>> GetExerciseAsync(string? Name, IList<int>? MuscleId, int page, int size, CancellationToken cancellationToken);

        public Task FillCacheFromDb(CancellationToken cancellationToken);
    }
}
