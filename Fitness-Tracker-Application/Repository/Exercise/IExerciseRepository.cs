using Fitness_Tracker_Domain.Entity;
using FluentResults;

namespace Fitness_Tracker_Application.Repository.Exercises
{
    public interface IExerciseRepository
    {
        public Task<Result<Exercise>> GetExerciseByIdAsync(int id, CancellationToken cancellationToken);

        public Task<IList<Exercise>> GetExerciseAsync(string? Name, IList<int>? MuscleId, CancellationToken cancellationToken);

        public Task<IList<Exercise>> GetAllExerciseAsync(CancellationToken cancellationToken);
    }
}
