using Fitness_Tracker_Application.Repository.Exercises;
using FluentResults;
using MediatR;

namespace Fitness_Tracker_Application.Features.Exercise
{
    public record FillCacheExerciseCommand() : IRequest<Result>;
    public class FillCacheExercise : IRequestHandler<FillCacheExerciseCommand, Result>
    {
        private readonly IExerciseRepository _repo;

        public FillCacheExercise(IExerciseRepository repo) 
        {
            _repo = repo;
        }

        public async Task<Result> Handle(FillCacheExerciseCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _repo.FillCacheFromDb(cancellationToken);
            }

            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }

            return Result.Ok();
        }
    }
}
