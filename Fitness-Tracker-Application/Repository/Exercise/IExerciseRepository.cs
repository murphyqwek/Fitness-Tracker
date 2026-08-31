using Fitness_Tracker_Application.DTO.Exercise;
using Fitness_Tracker_Application.Service.Pagination;
using FluentResults;

namespace Fitness_Tracker_Application.Repository.Exercises
{
    public interface IExerciseRepository
    {
        public Task<Result<ExerciseSearchDTO>> GetExerciseByIdAsync(int id, CancellationToken cancellationToken);

        public Task<PaginationResponse<ExerciseSearchReducedDTO>> GetExerciseAsync(string? Name, IList<int>? MuscleId, int page, int size, CancellationToken cancellationToken);

        public Task FillCacheFromDb(CancellationToken cancellationToken);

        public Task<bool> IsExerciseExist(int id, CancellationToken cancellationToken);

        public Task<bool> IsAllExercisesExist(List<int> ids, CancellationToken cancellationToken);
    }
}
