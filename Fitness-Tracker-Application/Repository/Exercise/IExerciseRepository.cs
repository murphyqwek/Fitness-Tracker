using Fitness_Tracker_Application.Features.Exercise;
using Fitness_Tracker_Domain.Entity;
using FluentResults;

namespace Fitness_Tracker_Application.Repository.Exercises
{
    public interface IExerciseRepository
    {
        public Task<Result<ExerciseSearchDTO>> GetExerciseByIdAsync(int id, CancellationToken cancellationToken);

        public Task<IList<ExerciseSearchDTO>> GetExerciseAsync(string? Name, IList<int>? MuscleId, CancellationToken cancellationToken);

        public Task<IList<ExerciseSearchDTO>> GetAllExerciseAsync(CancellationToken cancellationToken);
    }
}
